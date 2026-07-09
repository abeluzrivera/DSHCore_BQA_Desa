"""
Analiza logs de inicio de sesion (Logs/log-*.txt) y genera CSVs
con sesiones de usuario, resumen por usuario y alertas.

Uso:
    python main.py --logs-dir Logs --output sesiones.csv
"""

import argparse
import csv
import re
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path

LINE_RE = re.compile(
    r'^(?P<ts>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(?P<level>[A-Z]{3})\] (?P<msg>.*)$'
)

LOGIN_ATTEMPT_RE = re.compile(r'^Login attempt for user: (?P<user>\S+)$')
LDAP_OK_RE = re.compile(r"^LDAP OK: '(?P<user>[^']+)' → rol '(?P<rol>[^']+)'$")
LDAP_NO_GROUP_RE = re.compile(r"^LDAP: '(?P<user>[^']+)' autenticado pero sin grupo habilitado\.$")
LOGIN_ERROR_RE = re.compile(r'^Error during login$')
LOGGED_IN_RE = re.compile(r'^User (?P<user>\S+) logged in\.$')
DASHBOARD_RE = re.compile(r'^Dashboard accedido por el usuario: (?P<name>.+)$')
LOGOUT_RE = re.compile(r'^El usuario (?P<name>.+) está cerrando sesión$')

TS_FORMAT = "%Y-%m-%d %H:%M:%S.%f"
NAME_MAP_WINDOW_SECONDS = 5
ERROR_ATTRIBUTION_WINDOW_SECONDS = 30


def normalize_username(raw: str) -> str:
    return raw.split("@")[0].strip().lower()


@dataclass
class OpenSession:
    username: str
    login_dt: datetime
    rol: str = ""
    dashboard_hits: int = 0


@dataclass
class SessionRow:
    usuario: str
    nombre_completo: str
    rol: str
    fecha: str
    hora_login: str
    hora_logout: str
    duracion_minutos: str
    dashboard_accesos: int
    sesion_completa: str


@dataclass
class State:
    name_to_user: dict = field(default_factory=dict)
    user_to_name: dict = field(default_factory=dict)
    pending_rol: dict = field(default_factory=dict)
    open_sessions: dict = field(default_factory=dict)  # username -> list[OpenSession]
    sessions: list = field(default_factory=list)  # SessionRow
    login_attempts: dict = field(default_factory=dict)  # username -> count
    login_success: dict = field(default_factory=dict)  # username -> count
    no_group_alerts: dict = field(default_factory=dict)  # username -> count
    active_days: dict = field(default_factory=dict)  # username -> set of fechas
    alerts: list = field(default_factory=list)  # dict rows
    last_attempt: dict = field(default_factory=dict)  # username -> datetime (mas reciente)


def parse_timestamp(raw: str) -> datetime:
    return datetime.strptime(raw, TS_FORMAT)


def read_events(path: Path):
    """Genera tuplas (datetime, nivel, mensaje) por cada linea de log valida.
    Las lineas de continuacion (stack traces) no calzan con LINE_RE y se ignoran."""
    with path.open(encoding="utf-8", errors="replace") as f:
        for raw_line in f:
            line = raw_line.rstrip("\n").rstrip("\r")
            match = LINE_RE.match(line)
            if not match:
                continue
            yield parse_timestamp(match.group("ts")), match.group("level"), match.group("msg")


def close_session(state: State, username: str, logout_dt: datetime, nombre_completo: str):
    open_list = state.open_sessions.get(username)
    if open_list:
        session = open_list.pop()  # LIFO: el logout cierra el login abierto mas reciente
        duracion = (logout_dt - session.login_dt).total_seconds() / 60
        state.sessions.append(SessionRow(
            usuario=username,
            nombre_completo=nombre_completo or state.user_to_name.get(username, ""),
            rol=session.rol,
            fecha=session.login_dt.strftime("%Y-%m-%d"),
            hora_login=session.login_dt.strftime("%H:%M:%S"),
            hora_logout=logout_dt.strftime("%H:%M:%S"),
            duracion_minutos=f"{duracion:.2f}",
            dashboard_accesos=session.dashboard_hits,
            sesion_completa="SI",
        ))
    else:
        state.sessions.append(SessionRow(
            usuario=username,
            nombre_completo=nombre_completo,
            rol="",
            fecha=logout_dt.strftime("%Y-%m-%d"),
            hora_login="",
            hora_logout=logout_dt.strftime("%H:%M:%S"),
            duracion_minutos="",
            dashboard_accesos=0,
            sesion_completa="NO (sin login previo detectado)",
        ))


def process_file(path: Path, state: State):
    events = list(read_events(path))

    for idx, (dt, level, msg) in enumerate(events):
        m = LOGIN_ATTEMPT_RE.match(msg)
        if m:
            user = normalize_username(m.group("user"))
            state.login_attempts[user] = state.login_attempts.get(user, 0) + 1
            state.last_attempt[user] = dt
            continue

        m = LDAP_OK_RE.match(msg)
        if m:
            user = normalize_username(m.group("user"))
            state.pending_rol[user] = m.group("rol")
            continue

        m = LDAP_NO_GROUP_RE.match(msg)
        if m:
            user = normalize_username(m.group("user"))
            state.no_group_alerts[user] = state.no_group_alerts.get(user, 0) + 1
            state.alerts.append({
                "fecha_hora": dt.isoformat(sep=" "),
                "tipo": "sin_grupo_habilitado",
                "usuario": user,
                "detalle": "Autenticado en LDAP pero sin grupo habilitado en la aplicacion",
            })
            continue

        if LOGIN_ERROR_RE.match(msg):
            attributed_user = None
            best_dt = None
            for user, attempt_dt in state.last_attempt.items():
                if attempt_dt <= dt and (dt - attempt_dt).total_seconds() <= ERROR_ATTRIBUTION_WINDOW_SECONDS:
                    if best_dt is None or attempt_dt > best_dt:
                        best_dt = attempt_dt
                        attributed_user = user
            state.alerts.append({
                "fecha_hora": dt.isoformat(sep=" "),
                "tipo": "error_login",
                "usuario": attributed_user or "(desconocido)",
                "detalle": "Excepcion durante el proceso de login (ver log original para detalle)",
            })
            continue

        m = LOGGED_IN_RE.match(msg)
        if m:
            user = normalize_username(m.group("user"))
            state.login_success[user] = state.login_success.get(user, 0) + 1
            state.active_days.setdefault(user, set()).add(dt.strftime("%Y-%m-%d"))
            session = OpenSession(username=user, login_dt=dt, rol=state.pending_rol.get(user, ""))
            state.open_sessions.setdefault(user, []).append(session)

            # Correlacionar con la linea inmediata siguiente de "Dashboard accedido"
            # para mapear username <-> nombre completo (ver docstring de modulo).
            if idx + 1 < len(events):
                next_dt, _, next_msg = events[idx + 1]
                dm = DASHBOARD_RE.match(next_msg)
                if dm and (next_dt - dt).total_seconds() <= NAME_MAP_WINDOW_SECONDS:
                    name = dm.group("name").strip()
                    state.name_to_user.setdefault(name, user)
                    state.user_to_name.setdefault(user, name)
            continue

        m = DASHBOARD_RE.match(msg)
        if m:
            name = m.group("name").strip()
            user = state.name_to_user.get(name)
            if user:
                open_list = state.open_sessions.get(user)
                if open_list:
                    open_list[-1].dashboard_hits += 1
            continue

        m = LOGOUT_RE.match(msg)
        if m:
            name = m.group("name").strip()
            user = state.name_to_user.get(name, "")
            close_session(state, user or name, dt, name)
            continue


def build_summary_rows(state: State):
    usuarios = set(state.login_attempts) | set(state.login_success) | set(state.no_group_alerts)
    rows = []
    for user in sorted(usuarios):
        sesiones_usuario = [s for s in state.sessions if s.usuario == user]
        duraciones = [float(s.duracion_minutos) for s in sesiones_usuario if s.duracion_minutos]
        rol = next((s.rol for s in sesiones_usuario if s.rol), "")
        rows.append({
            "usuario": user,
            "nombre_completo": state.user_to_name.get(user, ""),
            "rol": rol,
            "dias_activos": len(state.active_days.get(user, set())),
            "logins_exitosos": state.login_success.get(user, 0),
            "intentos_login": state.login_attempts.get(user, 0),
            "intentos_fallidos": max(state.login_attempts.get(user, 0) - state.login_success.get(user, 0), 0),
            "sesiones_sin_cierre": sum(1 for s in sesiones_usuario if s.sesion_completa != "SI"),
            "dashboard_accesos_total": sum(s.dashboard_accesos for s in sesiones_usuario),
            "duracion_total_minutos": f"{sum(duraciones):.2f}" if duraciones else "",
            "duracion_promedio_minutos": f"{sum(duraciones) / len(duraciones):.2f}" if duraciones else "",
            "alertas_sin_grupo": state.no_group_alerts.get(user, 0),
        })
    return rows


def write_csv(path: Path, fieldnames, rows):
    with path.open("w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow(row)


def main():
    parser = argparse.ArgumentParser(description="Analiza logs de inicio de sesion de usuarios.")
    parser.add_argument("--logs-dir", default="Logs", help="Carpeta con los archivos log-*.txt (default: Logs)")
    parser.add_argument("--output", default="sesiones.csv", help="CSV de sesiones a generar (default: sesiones.csv)")
    args = parser.parse_args()

    logs_dir = Path(args.logs_dir)
    output_path = Path(args.output)
    if not logs_dir.is_dir():
        raise SystemExit(f"No existe la carpeta de logs: {logs_dir}")

    log_files = sorted(logs_dir.glob("*.txt"))
    if not log_files:
        raise SystemExit(f"No se encontraron archivos .txt en: {logs_dir}")

    state = State()
    for path in log_files:
        process_file(path, state)

    # Cerrar sesiones abiertas remanentes (sin logout detectado en los logs disponibles).
    for user, open_list in state.open_sessions.items():
        for session in open_list:
            state.sessions.append(SessionRow(
                usuario=user,
                nombre_completo=state.user_to_name.get(user, ""),
                rol=session.rol,
                fecha=session.login_dt.strftime("%Y-%m-%d"),
                hora_login=session.login_dt.strftime("%H:%M:%S"),
                hora_logout="",
                duracion_minutos="",
                dashboard_accesos=session.dashboard_hits,
                sesion_completa="NO (sin logout registrado)",
            ))
    state.open_sessions = {}

    session_fieldnames = [
        "usuario", "nombre_completo", "rol", "fecha", "hora_login", "hora_logout",
        "duracion_minutos", "dashboard_accesos", "sesion_completa",
    ]
    sessions_sorted = sorted(state.sessions, key=lambda s: (s.fecha, s.hora_login or "99"))
    write_csv(output_path, session_fieldnames, [s.__dict__ for s in sessions_sorted])

    summary_path = output_path.with_name(f"{output_path.stem}_resumen{output_path.suffix}")
    summary_fieldnames = [
        "usuario", "nombre_completo", "rol", "dias_activos", "logins_exitosos", "intentos_login",
        "intentos_fallidos", "sesiones_sin_cierre", "dashboard_accesos_total",
        "duracion_total_minutos", "duracion_promedio_minutos", "alertas_sin_grupo",
    ]
    write_csv(summary_path, summary_fieldnames, build_summary_rows(state))

    alerts_path = output_path.with_name(f"{output_path.stem}_alertas{output_path.suffix}")
    alerts_fieldnames = ["fecha_hora", "tipo", "usuario", "detalle"]
    alerts_sorted = sorted(state.alerts, key=lambda a: a["fecha_hora"])
    write_csv(alerts_path, alerts_fieldnames, alerts_sorted)

    print(f"Archivos procesados: {len(log_files)}")
    print(f"Sesiones escritas: {output_path} ({len(sessions_sorted)} filas)")
    print(f"Resumen por usuario: {summary_path}")
    print(f"Alertas: {alerts_path} ({len(alerts_sorted)} filas)")


if __name__ == "__main__":
    main()

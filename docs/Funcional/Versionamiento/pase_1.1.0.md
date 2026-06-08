# Pase 1.1.0

**Fecha:** 2026-06-02

---

## Mejoras

### Inicio de la aplicacion mas limpio y confiable

Al arrancar el sistema, se ejecuta una verificacion interna que compara los catalogos configurados en la base de datos contra los que el sistema conoce. Esta verificacion generaba 64 alertas falsas cada vez que se iniciaba la aplicacion, incluso cuando todo estaba correctamente configurado.

Se corrigio la logica de comparacion. Ahora el sistema arranca sin alertas innecesarias y la verificacion refleja el estado real de los datos.

**Beneficio para el usuario:** el sistema inicia mas rapido y los registros de actividad son mas faciles de revisar cuando algo realmente falla.

---

### Correccion de error al cargar los catalogos al iniciar

Al arrancar el sistema, se producia un error que interrumpia la carga inicial de los catalogos. Esto obligaba al sistema a cargarlos mas tarde, cuando cada usuario los necesitara por primera vez, en lugar de tenerlos listos de antemano.

Se corrigio el orden en que se ejecutan las consultas de precarga, evitando que se ejecuten al mismo tiempo y se interfieran entre si.

**Beneficio para el usuario:** los catalogos estan disponibles de inmediato al ingresar al sistema, sin demoras ni errores visibles durante la navegacion.

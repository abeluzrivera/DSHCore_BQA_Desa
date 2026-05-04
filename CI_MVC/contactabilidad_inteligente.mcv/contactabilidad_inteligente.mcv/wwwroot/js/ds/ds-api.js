/**
 * DS API — Versión Optimizada con Axios
 */
DS.modules.register('api', function () {

    // 1. Crear instancia centralizada
    const axiosInstance = axios.create({
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest',
        },
        timeout: 15000, // 15 segundos de espera máxima
        withCredentials: true // Importante: Enviar cookies de autenticación
    });

    // 2. INTERCEPTOR DE PETICIÓN (Request): Inyectar Token
    axiosInstance.interceptors.request.use(config => {
        const token = DS.utils.getAntiForgeryToken();
        if (token) {
            config.headers['RequestVerificationToken'] = token;
        }
        DS.events.emit('api:request', { method: config.method, url: config.url });
        return config;
    });

    // 3. INTERCEPTOR DE RESPUESTA (Response): Errores y 401
    axiosInstance.interceptors.response.use(
        (response) => {
            DS.events.emit('api:response', { url: response.config.url, status: response.status });
            return response;
        },
        (error) => {
            const { response, config } = error;
            const silent = config.silent || false;

            // Manejo de Sesión Expirada (401)
            if (response?.status === 401) {
                DS.notify?.warning('Sesión expirada. Redirigiendo...', { duration: 3000 });
                setTimeout(() => { window.location.href = '/login'; }, 2000);
            }
            // Manejo de Errores de Red
            else if (!response) {
                if (!silent) DS.notify?.error('Sin conexión al servidor');
            }
            // Errores del Servidor (400, 500, etc)
            else {
                const errorMsg = response.data?.message ?? response.data?.title ?? `Error ${response.status}`;
                if (!silent) DS.notify?.error(errorMsg);
            }

            DS.events.emit('api:error', { url: config.url, error });
            return Promise.reject(error);
        }
    );

    // 4. API Pública (Mantiene compatibilidad con tu código actual)
    DS.api = {
        request: async (method, url, data, opts = {}) => {
            try {
                const response = await axiosInstance({
                    method,
                    url,
                    data, // Axios usa 'data' para el body
                    ...opts // Pasa silent, signal, etc.
                });
                return { ok: true, data: response.data, status: response.status };
            } catch (error) {
                return {
                    ok: false,
                    data: error.response?.data,
                    error: error.message,
                    status: error.response?.status ?? 0
                };
            }
        },

        get: (url, opts) => DS.api.request('GET', url, null, opts),
        post: (url, body, opts) => DS.api.request('POST', url, body, opts),
        put: (url, body, opts) => DS.api.request('PUT', url, body, opts),
        patch: (url, body, opts) => DS.api.request('PATCH', url, body, opts),
        delete: (url, opts) => DS.api.request('DELETE', url, null, opts),
    };
});
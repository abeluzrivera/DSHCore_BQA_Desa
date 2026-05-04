namespace contactabilidad_inteligente.mcv.Pages.Auth
{
    /// <summary>
    /// Security configuration and constants for the login page
    /// </summary>
    public static class SecurityConfig
    {
        // =============================================
        // PASSWORD REQUIREMENTS
        // =============================================

        /// <summary>
        /// Minimum password length
        /// </summary>
        public const int MinPasswordLength = 8;

        /// <summary>
        /// Maximum password length
        /// </summary>
        public const int MaxPasswordLength = 128;

        /// <summary>
        /// Require at least one uppercase letter
        /// </summary>
        public const bool RequireUppercase = true;

        /// <summary>
        /// Require at least one lowercase letter
        /// </summary>
        public const bool RequireLowercase = true;

        /// <summary>
        /// Require at least one digit (0-9)
        /// </summary>
        public const bool RequireDigit = true;

        /// <summary>
        /// Require at least one special character
        /// </summary>
        public const bool RequireSpecialChar = true;

        // =============================================
        // ACCOUNT LOCKOUT POLICY
        // =============================================

        /// <summary>
        /// Number of failed login attempts before lockout
        /// </summary>
        public const int MaxFailedLoginAttempts = 5;

        /// <summary>
        /// Duration of account lockout in minutes
        /// </summary>
        public const int LockoutDurationMinutes = 15;

        /// <summary>
        /// Allow user lockout (false = disabled)
        /// </summary>
        public const bool AllowUserLockout = true;

        // =============================================
        // SESSION CONFIGURATION
        // =============================================

        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        public const int SessionTimeoutMinutes = 30;

        /// <summary>
        /// Absolute session timeout in minutes (maximum session duration)
        /// </summary>
        public const int AbsoluteSessionTimeoutMinutes = 480; // 8 hours

        // =============================================
        // RATE LIMITING
        /// =============================================

        /// <summary>
        /// Maximum login attempts per IP per minute
        /// </summary>
        public const int MaxLoginAttemptsPerMinute = 5;

        /// <summary>
        /// Maximum login attempts per IP per hour
        /// </summary>
        public const int MaxLoginAttemptsPerHour = 20;

        // =============================================
        // EMAIL CONFIGURATION
        // =============================================

        /// <summary>
        /// Enable email verification for new accounts
        /// </summary>
        public const bool RequireEmailVerification = true;

        /// <summary>
        /// Email verification token expiration in hours
        /// </summary>
        public const int EmailVerificationTokenExpirationHours = 24;

        /// <summary>
        /// Password reset token expiration in hours
        /// </summary>
        public const int PasswordResetTokenExpirationHours = 1;

        // =============================================
        // TWO-FACTOR AUTHENTICATION
        // =============================================

        /// <summary>
        /// Enable 2FA requirement
        /// </summary>
        public const bool Enable2FA = false; // Set to true when implemented

        /// <summary>
        /// 2FA code length
        /// </summary>
        public const int TwoFactorCodeLength = 6;

        /// <summary>
        /// 2FA code expiration in minutes
        /// </summary>
        public const int TwoFactorCodeExpirationMinutes = 5;

        // =============================================
        // LOGGING & AUDIT
        // =============================================

        /// <summary>
        /// Enable login attempt logging
        /// </summary>
        public const bool LogLoginAttempts = true;

        /// <summary>
        /// Enable failed login logging
        /// </summary>
        public const bool LogFailedLogins = true;

        /// <summary>
        /// Enable successful login logging
        /// </summary>
        public const bool LogSuccessfulLogins = true;

        /// <summary>
        /// Log user IP address
        /// </summary>
        public const bool LogUserIpAddress = true;

        /// <summary>
        /// Log user agent (browser info)
        /// </summary>
        public const bool LogUserAgent = true;

        // =============================================
        // CSRF PROTECTION
        // =============================================

        /// <summary>
        /// CSRF token cookie name
        /// </summary>
        public const string CsrfTokenCookieName = "XSRF-TOKEN";

        /// <summary>
        /// CSRF header name
        /// </summary>
        public const string CsrfHeaderName = "X-CSRF-TOKEN";

        /// <summary>
        /// Enable CSRF protection (automatic in Razor Pages)
        /// </summary>
        public const bool EnableCsrfProtection = true;

        // =============================================
        // HTTPS & SECURITY HEADERS
        // =============================================

        /// <summary>
        /// Require HTTPS (enforce in production)
        /// </summary>
        public const bool RequireHttps = true;

        /// <summary>
        /// HSTS max age in seconds (1 year)
        /// </summary>
        public const int HstsMaxAgeSeconds = 31536000;

        /// <summary>
        /// Include subdomains in HSTS
        /// </summary>
        public const bool HstsIncludeSubdomains = true;

        /// <summary>
        /// Preload HSTS list
        /// </summary>
        public const bool HstsPreload = true;

        // =============================================
        // COOKIE CONFIGURATION
        // =============================================

        /// <summary>
        /// Cookie expiration in days for "Remember Me"
        /// </summary>
        public const int RememberMeCookieDays = 30;

        /// <summary>
        /// Set HttpOnly flag on authentication cookies
        /// </summary>
        public const bool CookieHttpOnly = true;

        /// <summary>
        /// Require secure (HTTPS) for cookies
        /// </summary>
        public const bool CookieSecure = true;

        /// <summary>
        /// SameSite attribute for cookies (Strict, Lax, None)
        /// </summary>
        public const string CookieSameSite = "Strict";

        // =============================================
        // CONTENT SECURITY POLICY
        // =============================================

        /// <summary>
        /// Enable Content Security Policy
        /// </summary>
        public const bool EnableCsp = true;

        /// <summary>
        /// CSP policy directives
        /// </summary>
        public static readonly string CspPolicy = "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' cdn.jsdelivr.net fonts.googleapis.com; " +
            "style-src 'self' 'unsafe-inline' cdn.jsdelivr.net fonts.googleapis.com; " +
            "font-src 'self' fonts.gstatic.com; " +
            "img-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        // =============================================
        // VALIDATION MESSAGES
        // =============================================

        public static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "InvalidCredentials", "Correo electrónico o contraseña inválidos." },
            { "AccountLocked", "La cuenta está bloqueada. Intente más tarde." },
            { "InvalidEmail", "El formato del correo electrónico no es válido." },
            { "WeakPassword", "La contraseña no cumple con los requisitos de seguridad." },
            { "DuplicateEmail", "Ya existe una cuenta con este correo electrónico." },
            { "UserNotFound", "Usuario no encontrado." },
            { "SessionExpired", "Su sesión ha expirado. Por favor, inicie sesión nuevamente." },
            { "TooManyAttempts", "Demasiados intentos fallidos. Intente más tarde." },
            { "InternalError", "Error interno del servidor. Por favor, intente más tarde." },
            { "EmailNotVerified", "Por favor, verifique su correo electrónico antes de iniciar sesión." },
            { "TwoFactorRequired", "Autenticación de dos factores requerida." }
        };

        // =============================================
        // HELPER METHODS
        // =============================================

        /// <summary>
        /// Validate password strength
        /// </summary>
        public static (bool IsValid, List<string> Errors) ValidatePasswordStrength(string password)
        {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("La contraseña no puede estar vacía.");
                return (false, errors);
            }

            if (password.Length < MinPasswordLength)
                errors.Add($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");

            if (password.Length > MaxPasswordLength)
                errors.Add($"La contraseña no puede exceder {MaxPasswordLength} caracteres.");

            if (RequireUppercase && !password.Any(char.IsUpper))
                errors.Add("La contraseña debe contener al menos una letra mayúscula.");

            if (RequireLowercase && !password.Any(char.IsLower))
                errors.Add("La contraseña debe contener al menos una letra minúscula.");

            if (RequireDigit && !password.Any(char.IsDigit))
                errors.Add("La contraseña debe contener al menos un número.");

            if (RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("La contraseña debe contener al menos un carácter especial.");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmailFormat(string email)
        {
            try
            {
                System.Net.Mail.MailAddress addr = new(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get security headers for HTTP response
        /// </summary>
        public static Dictionary<string, string> GetSecurityHeaders()
        {
            return new Dictionary<string, string>
            {
                { "X-Content-Type-Options", "nosniff" },
                { "X-Frame-Options", "DENY" },
                { "X-XSS-Protection", "1; mode=block" },
                { "Referrer-Policy", "strict-origin-when-cross-origin" },
                { "Permissions-Policy", "geolocation=(), microphone=(), camera=()" },
                { "Strict-Transport-Security", $"max-age={HstsMaxAgeSeconds}; includeSubDomains; preload" },
                { "Content-Security-Policy", CspPolicy }
            };
        }
    }
}

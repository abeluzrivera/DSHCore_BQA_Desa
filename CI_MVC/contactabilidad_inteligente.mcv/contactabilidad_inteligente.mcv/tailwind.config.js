/** @type {import('tailwindcss').Config} */
module.exports = {
  // Scan all Razor pages, views, CS files and JS for class usage
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Areas/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './**/*.cs',
  ],

  // Prevent Tailwind from resetting Bootstrap styles on shared classes
  // Use prefix 'tw-' so Tailwind utilities never collide with Bootstrap classes
  prefix: 'tw-',

  theme: {
    extend: {
      fontFamily: {
        sora: ['Inter', 'sans-serif'],
      },
      colors: {
        // Design tokens — espejo de variables.css
        primary: {
          dark:     '#295136',   // --color-primary-dark (Deep Forest)
          DEFAULT:  '#437557',   // --color-primary
          light:    '#5A8D6E',   // --color-primary-light
          pale:     '#7F9787',   // --color-primary-pale (Muted Sage)
          surface:  '#ebf2ee',   // --color-bg-selected  (fila activa lista)
        },
        accent: {
          DEFAULT: '#CB801B',   // --color-alert (Amber Ochre)
          hover:   '#A86915',   // --ocre-hover
        },
        bg: {
          light:    '#DBE4DD',   // --color-bg-light
          subtle:   '#EDF2EE',   // --color-bg-subtle
          selected: '#ebf2ee',   // --color-bg-selected
          page:     '#f5f7f5',   // --color-bg-page
        },
        border: {
          DEFAULT: 'rgba(41,81,54,0.12)',  // --color-border
          list:    'rgba(41,81,54,0.05)',  // --color-border-list
          strong:  'rgba(41,81,54,0.25)',  // --color-border-strong
        },
        danger:  '#DC2626',
        warning: '#F59E0B',
        success: '#16A34A',
        // Section - Light Green Form Area tokens
        'form-light':      '#d8e2dc',    // Main section background
        'form-lighter':    '#cfe2d6',    // Alert box background
        'form-icon-bg':    '#f0fdf4',    // Icon container background
        'form-border':     '#7f9787',    // Dropzone border color
        'text-dark-body':  '#1b2b21',    // File name and body text
        'text-body-muted': '#334155',    // Muted secondary text
      },
      borderRadius: {
        sm:  '4px',
        DEFAULT: '8px',
        lg:  '12px',
        xl:  '16px',
        '2xl': '24px',
      },
      boxShadow: {
        card: '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
        sm:   '0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)',
      },
      transitionDuration: {
        fast: '150ms',
        base: '200ms',
          },
          verified: {
              bg: 'var(--color-verified-bg)',
              border: 'var(--color-verified-border)',
              text: 'var(--color-verified-text)',
          },
          pending: {
              bg: 'var(--color-pending-bg)',
              border: 'var(--color-pending-border)',
              text: 'var(--color-pending-text)',
          },
          unverified: {
              bg: 'var(--color-unverified-bg)',
              border: 'var(--color-rejection)', // Asumo que el borde usa el rojo fuerte
              text: 'var(--color-rejection)',
          }
    },
  },

  plugins: [],

  // Leave Bootstrap's own utility names alone — important while migrating
  corePlugins: {
    preflight: false,  // Do NOT inject Tailwind reset; Bootstrap has its own
  },
};

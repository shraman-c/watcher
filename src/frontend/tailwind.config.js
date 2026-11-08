/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        bg: {
          dark: '#111318',
          light: '#F8FAFC',
        },
        sidebar: {
          dark: '#181B22',
          light: '#FFFFFF',
        },
        card: {
          dark: '#20242D',
          light: '#FFFFFF',
        },
        elevated: {
          dark: '#272C36',
          light: '#F1F5F9',
        },
        border: {
          dark: '#2B313E',
          light: '#E2E8F0',
        },
        accent: {
          DEFAULT: '#6C9BFF',
          hover: '#5287F6',
          muted: 'rgba(108, 155, 255, 0.15)',
        },
        success: {
          DEFAULT: '#35B77A',
          muted: 'rgba(53, 183, 122, 0.15)',
        },
        warning: {
          DEFAULT: '#E9AE54',
          muted: 'rgba(233, 174, 84, 0.15)',
        },
        danger: {
          DEFAULT: '#E36F75',
          muted: 'rgba(227, 111, 117, 0.15)',
        },
        foreground: {
          primary: {
            dark: '#F3F5F8',
            light: '#0F172A',
          },
          secondary: {
            dark: '#A0A8B5',
            light: '#64748B',
          },
          muted: {
            dark: '#677182',
            light: '#94A3B8',
          },
        },
      },
      fontFamily: {
        sans: ['Segoe UI Variable', 'Segoe UI', '-apple-system', 'BlinkMacSystemFont', 'Inter', 'Roboto', 'sans-serif'],
      },
      borderRadius: {
        fluent: '8px',
        card: '12px',
        panel: '16px',
      },
      boxShadow: {
        subtle: '0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.08)',
        card: '0 4px 12px rgba(0, 0, 0, 0.15), 0 1px 2px rgba(0, 0, 0, 0.1)',
        flyout: '0 12px 32px rgba(0, 0, 0, 0.35)',
      },
    },
  },
  plugins: [],
};


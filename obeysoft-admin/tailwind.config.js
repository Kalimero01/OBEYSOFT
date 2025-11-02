/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{ts,tsx}"
  ],
  theme: {
    extend: {
      colors: {
        panel: "#0f141b",
        surface: "#121824",
        primary: {
          DEFAULT: "#3b82f6",
          fg: "#0b1220"
        }
      }
    }
  },
  plugins: []
}



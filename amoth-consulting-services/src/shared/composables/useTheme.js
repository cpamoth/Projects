import { ref, onMounted, watch } from 'vue'

export function useTheme() {
  const theme = ref('dark')

  const toggleTheme = () => {
    theme.value = theme.value === 'light' ? 'dark' : 'light'
  }

  onMounted(() => {
    const storedTheme = localStorage.getItem('theme')
    if (storedTheme === 'dark' || storedTheme === 'light') {
      theme.value = storedTheme
    }
  })

  watch(
    theme,
    (value) => {
      document.documentElement.dataset.theme = value
      localStorage.setItem('theme', value)
    },
    { immediate: true }
  )

  return { theme, toggleTheme }
}

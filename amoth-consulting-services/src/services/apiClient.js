const apiUrl = (import.meta.env.VITE_API_URL || 'http://localhost:3001').replace(/\/$/, '')

function getCsrfToken() {
  return document.cookie.split('; ').find((entry) => entry.startsWith('csrf='))?.split('=').slice(1).join('=') || ''
}

export async function apiRequest(path, options = {}) {
  const csrfToken = getCsrfToken()
  const response = await fetch(`${apiUrl}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(csrfToken ? { 'X-CSRF-Token': csrfToken } : {}),
      ...options.headers,
    },
  })

  const payload = await response.json().catch(() => ({}))
  if (!response.ok) {
    const error = new Error(payload.error || 'Request failed')
    Object.assign(error, payload)
    throw error
  }
  return payload
}

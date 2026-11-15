import { apiRequest } from './apiClient.js'

export const login = (username, password, mfaToken) => apiRequest('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ username, password, ...(mfaToken ? { mfaToken } : {}) })
})

export const register = (user) => apiRequest('/api/auth/register', {
  method: 'POST',
  body: JSON.stringify(user)
})

export const requestPasswordReset = (email) => apiRequest('/api/auth/password-reset/request', {
  method: 'POST',
  body: JSON.stringify({ email })
})

export const resetPassword = (token, password) => apiRequest('/api/auth/password-reset/confirm', {
  method: 'POST',
  body: JSON.stringify({ token, password })
})

export const logout = () => apiRequest('/api/auth/logout', { method: 'POST' })

export const changePassword = (passwords) => apiRequest('/api/change-password', {
  method: 'POST',
  body: JSON.stringify(passwords)
})

export const setupMfa = () => apiRequest('/api/auth/mfa/setup', { method: 'POST' })
export const enableMfa = (token) => apiRequest('/api/auth/mfa/enable', { method: 'POST', body: JSON.stringify({ token }) })

export const setAuthUser = (user) => sessionStorage.setItem('authUser', JSON.stringify(user))
export const getAuthUser = () => {
  try { return JSON.parse(sessionStorage.getItem('authUser') || 'null') } catch { return null }
}
export const clearAuthUser = () => sessionStorage.removeItem('authUser')
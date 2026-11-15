import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  changePassword,
  clearAuthUser,
  enableMfa,
  getAuthUser,
  login,
  logout,
  register,
  requestPasswordReset,
  resetPassword,
  setAuthUser,
  setupMfa,
} from '../src/services/authApi.js'

function mockResponse(payload, ok = true) {
  return {
    ok,
    json: vi.fn().mockResolvedValue(payload),
  }
}

describe('authApi', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
    sessionStorage.clear()
    document.cookie = 'csrf=; Max-Age=0; path=/'
  })

  it('logs in with credentials and the MFA token when provided', async () => {
    document.cookie = 'csrf=csrf-token; path=/'
    const response = mockResponse({ user: { id: 4, username: 'jane' } })
    const fetchMock = vi.fn().mockResolvedValue(response)
    vi.stubGlobal('fetch', fetchMock)

    await login('jane', 'secure-password', '123456')

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username: 'jane', password: 'secure-password', mfaToken: '123456' }),
      credentials: 'include',
      headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': 'csrf-token' },
    })
  })

  it('registers without sending a CSRF header before a session exists', async () => {
    const response = mockResponse({ user: { id: 5, username: 'new-user' } })
    const fetchMock = vi.fn().mockResolvedValue(response)
    vi.stubGlobal('fetch', fetchMock)

    await register({ username: 'new-user', email: 'new@example.com', password: 'secure-password' })

    expect(fetchMock.mock.calls[0][1].headers).toEqual({ 'Content-Type': 'application/json' })
  })

  it('requests a password reset and confirms it with the emailed token', async () => {
    const fetchMock = vi.fn().mockResolvedValue(mockResponse({ message: 'ok' }))
    vi.stubGlobal('fetch', fetchMock)

    await requestPasswordReset('jane@example.com')
    await resetPassword('one-time-token', 'new-password-123')

    expect(fetchMock.mock.calls[0]).toEqual([
      '/api/auth/password-reset/request',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ email: 'jane@example.com' }) }),
    ])
    expect(fetchMock.mock.calls[1]).toEqual([
      '/api/auth/password-reset/confirm',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ token: 'one-time-token', password: 'new-password-123' }) }),
    ])
  })

  it('preserves MFA challenge metadata on failed login', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(
      mockResponse({ error: 'MFA token required or invalid', mfaRequired: true }, false)
    ))

    await expect(login('jane', 'secure-password')).rejects.toMatchObject({
      message: 'MFA token required or invalid',
      mfaRequired: true,
    })
  })

  it('sends CSRF protection for logout, password changes, and MFA enablement', async () => {
    document.cookie = 'csrf=csrf-token; path=/'
    const fetchMock = vi.fn().mockResolvedValue(mockResponse({ message: 'ok' }))
    vi.stubGlobal('fetch', fetchMock)

    await logout()
    await changePassword({ currentPassword: 'old-password', newPassword: 'new-password', confirmPassword: 'new-password' })
    await setupMfa()
    await enableMfa('123456')

    expect(fetchMock).toHaveBeenCalledTimes(4)
    for (const [, options] of fetchMock.mock.calls) {
      expect(options.credentials).toBe('include')
      expect(options.headers['X-CSRF-Token']).toBe('csrf-token')
    }
  })

  it('stores, reads, and clears only the non-secret user profile marker', () => {
    const user = { id: 4, username: 'jane', fullName: 'Jane Doe' }

    setAuthUser(user)
    expect(getAuthUser()).toEqual(user)

    clearAuthUser()
    expect(getAuthUser()).toBeNull()
  })

  it('returns null when the stored user marker is malformed', () => {
    sessionStorage.setItem('authUser', '{invalid-json')

    expect(getAuthUser()).toBeNull()
  })
})
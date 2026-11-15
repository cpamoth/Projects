import { beforeEach, describe, expect, it, vi } from 'vitest'

const { mockDb, mockHashPassword, mockComparePassword, mockGenerateToken, mockAssertEmailConfigured, mockSendPasswordResetEmail } = vi.hoisted(() => ({
  mockDb: { get: vi.fn(), run: vi.fn() },
  mockHashPassword: vi.fn(),
  mockComparePassword: vi.fn(),
  mockGenerateToken: vi.fn(),
  mockAssertEmailConfigured: vi.fn(),
  mockSendPasswordResetEmail: vi.fn()
}))

vi.mock('../../server/src/database/db.js', () => ({
  getDb: () => mockDb
}))

vi.mock('../../server/src/utils/auth.js', () => ({
  generateToken: mockGenerateToken,
  comparePassword: mockComparePassword,
  hashPassword: mockHashPassword
}))

vi.mock('../../server/src/utils/email.js', () => ({
  assertEmailConfigured: mockAssertEmailConfigured,
  sendPasswordResetEmail: mockSendPasswordResetEmail
}))

import { loginRoute, registerRoute, requestPasswordResetRoute, resetPasswordRoute } from '../../server/src/routes/auth.js'

function createReply() {
  return {
    statusCode: 200,
    cookies: [],
    status(code) {
      this.statusCode = code
      return this
    },
    send(payload) {
      this.payload = payload
      return payload
    },
    setCookie(name, value, options) {
      this.cookies.push({ name, value, options })
      return this
    }
  }
}

function createFastify() {
  const routes = {}
  return {
    post(path, optionsOrHandler, maybeHandler) {
      routes[path] = maybeHandler || optionsOrHandler
    },
    routes
  }
}

describe('auth routes', () => {
  beforeEach(() => {
    mockDb.get.mockReset()
    mockDb.run.mockReset()
    mockHashPassword.mockReset()
    mockComparePassword.mockReset()
    mockGenerateToken.mockReset()
    mockAssertEmailConfigured.mockReset()
    mockSendPasswordResetEmail.mockReset()
  })

  it('rejects registration when required fields are missing', async () => {
    const fastify = createFastify()
    await registerRoute(fastify)

    const reply = createReply()
    const result = await fastify.routes['/register']({ body: { username: 'demo' } }, reply)

    expect(reply.statusCode).toBe(400)
    expect(result.error).toBe('Username, email, and password are required')
  })

  it('registers a new user and sets a session cookie', async () => {
    const fastify = createFastify()
    await registerRoute(fastify)

    mockDb.get.mockResolvedValueOnce(null)
    mockHashPassword.mockResolvedValueOnce('hashed-password')
    mockDb.run.mockResolvedValueOnce({ lastID: 7 })
    mockGenerateToken.mockReturnValueOnce('generated-token')

    const reply = createReply()
    const result = await fastify.routes['/register']({
      body: {
        username: 'demo',
        email: 'demo@example.com',
        password: 'secure-password',
        fullName: 'Demo User'
      }
    }, reply)

    expect(reply.statusCode).toBe(201)
    expect(result.token).toBeUndefined()
    expect(reply.cookies.find((cookie) => cookie.name === 'session').value).toBe('generated-token')
    expect(reply.cookies.find((cookie) => cookie.name === 'csrf')).toBeTruthy()
    expect(result.user.username).toBe('demo')
    expect(mockHashPassword).toHaveBeenCalledWith('secure-password')
    expect(mockDb.run).toHaveBeenCalled()
  })

  it('logs in an existing user with the correct password', async () => {
    const fastify = createFastify()
    await loginRoute(fastify)

    mockDb.get.mockResolvedValueOnce({
      id: 3,
      username: 'demo',
      email: 'demo@example.com',
      password: 'hashed-password',
      fullName: 'Demo User',
      bio: 'Bio'
    })
    mockComparePassword.mockResolvedValueOnce(true)
    mockGenerateToken.mockReturnValueOnce('login-token')

    const reply = createReply()
    const result = await fastify.routes['/login']({
      body: { username: 'demo', password: 'secure-password' }
    }, reply)

    expect(reply.statusCode).toBe(200)
    expect(result.token).toBeUndefined()
    expect(reply.cookies.find((cookie) => cookie.name === 'session').value).toBe('login-token')
    expect(reply.cookies.find((cookie) => cookie.name === 'csrf')).toBeTruthy()
    expect(result.user.username).toBe('demo')
    expect(mockComparePassword).toHaveBeenCalledWith('secure-password', 'hashed-password')
  })

  it('sends a reset link with a stored hash for a known email', async () => {
    const fastify = createFastify()
    await requestPasswordResetRoute(fastify)
    mockDb.get.mockResolvedValueOnce({ id: 8, email: 'jane@example.com' })
    mockDb.run.mockResolvedValueOnce({ lastID: 8 })
    mockSendPasswordResetEmail.mockResolvedValueOnce()

    const reply = createReply()
    const result = await fastify.routes['/password-reset/request']({
      body: { email: 'jane@example.com' }
    }, reply)

    expect(reply.statusCode).toBe(200)
    expect(result.message).toContain('If an account exists')
    const resetUrl = new URL(mockSendPasswordResetEmail.mock.calls[0][0].resetUrl)
    expect(resetUrl.searchParams.get('resetToken')).toMatch(/^[\w-]{43}$/)
    expect(mockDb.run.mock.calls[0][1][1]).not.toBe(resetUrl.searchParams.get('resetToken'))
  })

  it('returns the same reset response when the email is unknown', async () => {
    const fastify = createFastify()
    await requestPasswordResetRoute(fastify)
    mockDb.get.mockResolvedValueOnce(null)

    const reply = createReply()
    const result = await fastify.routes['/password-reset/request']({
      body: { email: 'unknown@example.com' }
    }, reply)

    expect(reply.statusCode).toBe(200)
    expect(result.message).toContain('If an account exists')
    expect(mockSendPasswordResetEmail).not.toHaveBeenCalled()
  })

  it('reports an unavailable reset service when email delivery is not configured', async () => {
    const fastify = createFastify()
    await requestPasswordResetRoute(fastify)
    const configurationError = new Error('SMTP is not configured')
    configurationError.code = 'EMAIL_CONFIGURATION'
    mockAssertEmailConfigured.mockImplementationOnce(() => { throw configurationError })

    const reply = createReply()
    const result = await fastify.routes['/password-reset/request']({
      body: { email: 'jane@example.com' }
    }, reply)

    expect(reply.statusCode).toBe(503)
    expect(result.error).toContain('not configured')
    expect(mockDb.get).not.toHaveBeenCalled()
  })

  it('updates the password only when a valid one-time reset token is consumed', async () => {
    const fastify = createFastify()
    await resetPasswordRoute(fastify)
    mockHashPassword.mockResolvedValueOnce('new-hashed-password')
    mockDb.run.mockResolvedValueOnce({ lastID: 8 })

    const reply = createReply()
    const result = await fastify.routes['/password-reset/confirm']({
      body: { token: 'a-valid-one-time-reset-token-12345678901234567890', password: 'new-password-123' }
    }, reply)

    expect(reply.statusCode).toBe(200)
    expect(result.message).toContain('Password reset successfully')
    expect(mockHashPassword).toHaveBeenCalledWith('new-password-123')
    expect(mockDb.run.mock.calls[0][0]).toContain('DELETE FROM password_reset_tokens')
  })

  it('rejects expired or already-used reset tokens', async () => {
    const fastify = createFastify()
    await resetPasswordRoute(fastify)
    mockHashPassword.mockResolvedValueOnce('new-hashed-password')
    mockDb.run.mockResolvedValueOnce({ lastID: undefined })

    const reply = createReply()
    const result = await fastify.routes['/password-reset/confirm']({
      body: { token: 'a-valid-one-time-reset-token-12345678901234567890', password: 'new-password-123' }
    }, reply)

    expect(reply.statusCode).toBe(400)
    expect(result.error).toContain('invalid or has expired')
  })
})

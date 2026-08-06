import { beforeEach, describe, expect, it, vi } from 'vitest'

const { mockDb, mockHashPassword, mockComparePassword, mockGenerateToken } = vi.hoisted(() => ({
  mockDb: { get: vi.fn(), run: vi.fn() },
  mockHashPassword: vi.fn(),
  mockComparePassword: vi.fn(),
  mockGenerateToken: vi.fn()
}))

vi.mock('../../server/src/database/db.js', () => ({
  getDb: () => mockDb
}))

vi.mock('../../server/src/utils/auth.js', () => ({
  generateToken: mockGenerateToken,
  comparePassword: mockComparePassword,
  hashPassword: mockHashPassword
}))

import { loginRoute, registerRoute } from '../../server/src/routes/auth.js'

function createReply() {
  return {
    statusCode: 200,
    status(code) {
      this.statusCode = code
      return this
    },
    send(payload) {
      this.payload = payload
      return payload
    }
  }
}

function createFastify() {
  const routes = {}
  return {
    post(path, handler) {
      routes[path] = handler
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
  })

  it('rejects registration when required fields are missing', async () => {
    const fastify = createFastify()
    await registerRoute(fastify)

    const reply = createReply()
    const result = await fastify.routes['/register']({ body: { username: 'demo' } }, reply)

    expect(reply.statusCode).toBe(400)
    expect(result.error).toBe('Username, email, and password are required')
  })

  it('registers a new user and returns a token', async () => {
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
        password: 'secure-pass',
        fullName: 'Demo User'
      }
    }, reply)

    expect(reply.statusCode).toBe(201)
    expect(result.token).toBe('generated-token')
    expect(result.user.username).toBe('demo')
    expect(mockHashPassword).toHaveBeenCalledWith('secure-pass')
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
      body: { username: 'demo', password: 'secure-pass' }
    }, reply)

    expect(reply.statusCode).toBe(200)
    expect(result.token).toBe('login-token')
    expect(result.user.username).toBe('demo')
    expect(mockComparePassword).toHaveBeenCalledWith('secure-pass', 'hashed-password')
  })
})

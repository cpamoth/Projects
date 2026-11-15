import { beforeEach, describe, expect, it, vi } from 'vitest'

const { mockConnect, mockEnd, mockQuery, mockRelease, mockPool } = vi.hoisted(() => {
  const mockQuery = vi.fn()
  const mockRelease = vi.fn()
  const mockConnect = vi.fn()
  const mockEnd = vi.fn()
  const mockPool = vi.fn(() => ({
    connect: mockConnect,
    end: mockEnd,
    query: mockQuery
  }))

  return { mockConnect, mockEnd, mockQuery, mockRelease, mockPool }
})

import { closeDb, getDb, initDb } from '../../server/src/database/db.js'

describe('PostgreSQL database connection', () => {
  beforeEach(async () => {
    await closeDb()
    mockConnect.mockReset()
    mockEnd.mockReset()
    mockQuery.mockReset()
    mockRelease.mockReset()
    mockPool.mockClear()
    mockConnect.mockResolvedValue({ query: mockQuery, release: mockRelease })
    mockQuery.mockResolvedValue({ rows: [], rowCount: 0 })
  })

  it('initializes a PostgreSQL pool without mutating the schema', async () => {
    const pool = await initDb({ poolFactory: mockPool })

    expect(pool).toBeDefined()
    expect(mockPool).toHaveBeenCalledTimes(1)
    expect(mockConnect).not.toHaveBeenCalled()
    expect(mockQuery).not.toHaveBeenCalled()
  })

  it('translates route queries to PostgreSQL parameters', async () => {
    await initDb({ poolFactory: mockPool })
    mockQuery.mockResolvedValueOnce({ rows: [{ id: 7 }], rowCount: 1 })

    const db = getDb()
    const result = await db.run(
      'INSERT INTO users (username, email) VALUES (?, ?)',
      ['demo', 'demo@example.com']
    )

    expect(result).toEqual({ lastID: 7, changes: 1 })
    expect(mockQuery).toHaveBeenLastCalledWith(
      'INSERT INTO users (username, email) VALUES ($1, $2) RETURNING id',
      ['demo', 'demo@example.com']
    )
  })

  it('closes the PostgreSQL pool cleanly', async () => {
    await initDb({ poolFactory: mockPool })

    await closeDb()

    expect(mockEnd).toHaveBeenCalledTimes(1)
  })
})

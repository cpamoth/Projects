import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { cleanup, fireEvent, render, screen } from '@testing-library/vue'
import NavBar from '../shared/components/NavBar.vue'

vi.mock('../services/authApi.js', () => ({
  clearAuthUser: vi.fn(() => sessionStorage.removeItem('authUser')),
  getAuthUser: vi.fn(() => JSON.parse(sessionStorage.getItem('authUser') || 'null')),
  logout: vi.fn().mockResolvedValue(undefined),
}))

const renderNavBar = () => render(NavBar, {
  props: { theme: 'light' },
  global: {
    stubs: {
      RouterLink: {
        props: ['to'],
        template: '<a :href="to"><slot /></a>',
      },
    },
  },
})

describe('NavBar.vue', () => {
  beforeEach(() => {
    localStorage.clear()
    sessionStorage.clear()
  })

  afterEach(() => {
    cleanup()
  })

  it('shows profile full name when authenticated', async () => {
    sessionStorage.setItem('authUser', JSON.stringify({ fullName: 'Jane Doe', username: 'jdoe' }))

    renderNavBar()

    expect(await screen.findByText(/Jane Doe/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /sign out/i })).toBeTruthy()
  })

  it('falls back to username when full name is not available', async () => {
    sessionStorage.setItem('authUser', JSON.stringify({ username: 'jdoe', fullName: '' }))

    renderNavBar()

    expect(await screen.findByText(/jdoe/i)).toBeTruthy()
  })

  it('removes token on sign out', async () => {
    sessionStorage.setItem('authUser', JSON.stringify({ fullName: 'Jane Doe', username: 'jdoe' }))

    renderNavBar()

    const logoutButton = screen.getAllByRole('button', { name: /sign out/i })[0]
    await fireEvent.click(logoutButton)
    await nextTick()

    expect(sessionStorage.getItem('authUser')).toBeNull()
  })
})

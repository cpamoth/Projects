import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { cleanup, fireEvent, render, screen } from '@testing-library/vue'
import NavBar from '../shared/components/NavBar.vue'

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
  })

  afterEach(() => {
    cleanup()
  })

  it('shows profile full name when authenticated', async () => {
    localStorage.setItem('token', btoa('jdoe:pass123'))
    localStorage.setItem('userProfile', JSON.stringify({ fullName: 'Jane Doe', username: 'jdoe' }))

    renderNavBar()

    expect(await screen.findByText(/Jane Doe/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /sign out/i })).toBeTruthy()
  })

  it('falls back to username when full name is not available', async () => {
    localStorage.setItem('token', btoa('jdoe:pass123'))
    localStorage.setItem('userProfile', JSON.stringify({ username: 'jdoe', fullName: '' }))

    renderNavBar()

    expect(await screen.findByText(/jdoe/i)).toBeTruthy()
  })

  it('removes token on sign out', async () => {
    localStorage.setItem('token', btoa('jdoe:pass123'))
    localStorage.setItem('userProfile', JSON.stringify({ fullName: 'Jane Doe', username: 'jdoe' }))

    renderNavBar()

    const logoutButton = screen.getAllByRole('button', { name: /sign out/i })[0]
    await fireEvent.click(logoutButton)

    expect(localStorage.getItem('token')).toBeNull()
  })
})

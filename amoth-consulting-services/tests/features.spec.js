import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { services as fallbackServices } from '../src/data/services.js'
import { getServiceById, getServices } from '../src/services/contentApi.js'
import Service from '../src/features/services/components/Service.vue'

vi.mock('../src/services/contentApi.js', () => ({
  getServices: vi.fn(),
  getServiceById: vi.fn(),
}))

vi.mock('../src/services/authApi.js', () => ({
  login: vi.fn().mockResolvedValue({ user: { username: 'jdoe', fullName: 'John Doe' } }),
  register: vi.fn().mockResolvedValue({ user: { username: 'janedoe', fullName: 'Jane Doe' } }),
  requestPasswordReset: vi.fn().mockResolvedValue({ message: 'If an account exists for that email, a password reset link has been sent.' }),
  resetPassword: vi.fn().mockResolvedValue({ message: 'Password reset successfully.' }),
  setAuthUser: vi.fn((user) => sessionStorage.setItem('authUser', JSON.stringify(user))),
  getAuthUser: vi.fn(() => JSON.parse(sessionStorage.getItem('authUser') || 'null')),
}))

vi.mock('../src/services/profileApi.js', () => ({
  getProfile: vi.fn().mockResolvedValue({
    id: 1,
    username: 'jdoe',
    email: 'john@example.com',
    fullName: 'John Doe',
    bio: 'Consultant',
  }),
  updateProfile: vi.fn(),
}))

const routerPushMock = vi.fn()
const routerReplaceMock = vi.fn()
const routeState = { params: { serviceId: 'strategy-transformation' }, path: '/', query: {} }

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: routerPushMock, replace: routerReplaceMock }),
  useRoute: () => routeState,
  RouterLink: {
    name: 'RouterLink',
    props: ['to'],
    template: '<a><slot /></a>',
  },
}))

import Login from '../src/features/auth/components/Login.vue'
import Register from '../src/features/auth/components/Register.vue'
import { requestPasswordReset, resetPassword } from '../src/services/authApi.js'
import ServiceDetail from '../src/features/services/components/ServiceDetail.vue'
import BillingOptions from '../src/features/profile/components/BillingOptions.vue'
import Profile from '../src/features/profile/components/Profile.vue'
import { getProfile, updateProfile } from '../src/services/profileApi.js'

const globalStubs = {
  PageWrapper: {
    template: '<div><slot /></div>',
  },
  RouterLink: {
    name: 'RouterLink',
    props: ['to'],
    template: '<a><slot /></a>',
  },
}

describe('feature behavior tests', () => {
  beforeEach(() => {
    localStorage.clear()
    sessionStorage.clear()
    routerPushMock.mockReset()
    routerReplaceMock.mockReset()
    routeState.params.serviceId = 'strategy-transformation'
    routeState.path = '/'
    routeState.query = {}
    vi.mocked(getServices).mockResolvedValue(fallbackServices)
    vi.mocked(getServiceById).mockImplementation(async (serviceId) => {
      return fallbackServices.find((service) => service.id === serviceId) || fallbackServices[0]
    })
    vi.mocked(getProfile).mockResolvedValue({
      id: 1,
      username: 'jdoe',
      email: 'john@example.com',
      fullName: 'John Doe',
      bio: 'Consultant',
    })
    vi.mocked(updateProfile).mockResolvedValue({
      message: 'Profile updated successfully',
      user: {
        id: 1,
        username: 'jdoe',
        email: 'updated@example.com',
        fullName: 'John Doe',
        bio: 'Updated bio',
      },
    })
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.runOnlyPendingTimers()
    vi.useRealTimers()
  })

  it('validates login input and stores auth data on success', async () => {
    const wrapper = mount(Login, { global: { stubs: globalStubs } })

    await wrapper.get('form').trigger('submit')
    expect(wrapper.text()).toContain('Password must be at least 6 characters')

    await wrapper.get('#username').setValue('jdoe')
    await wrapper.get('#password').setValue('secret-password')
    await wrapper.get('form').trigger('submit')

    await flushPromises()

    expect(wrapper.emitted('login')).toBeTruthy()
    expect(sessionStorage.getItem('authUser')).toBeTruthy()
    expect(routerPushMock).toHaveBeenCalledWith('/dashboard')
  })

  it('lets a user request a password reset link from the login screen', async () => {
    const wrapper = mount(Login, { global: { stubs: globalStubs } })
    await wrapper.get('.reset-link').trigger('click')
    await wrapper.get('#reset-email').setValue('jane@example.com')
    await wrapper.get('.reset-form').trigger('submit')
    await flushPromises()

    expect(requestPasswordReset).toHaveBeenCalledWith('jane@example.com')
    expect(wrapper.text()).toContain('If an account exists for that email')
  })

  it('accepts a new password from the email reset link', async () => {
    routeState.query = { resetToken: 'reset-token-from-email' }
    const wrapper = mount(Login, { global: { stubs: globalStubs } })
    await wrapper.get('#new-password').setValue('new-password-123')
    await wrapper.get('#confirm-password').setValue('new-password-123')
    await wrapper.get('.reset-form').trigger('submit')
    await flushPromises()

    expect(resetPassword).toHaveBeenCalledWith('reset-token-from-email', 'new-password-123')
    expect(wrapper.text()).toContain('Your password has been reset')
  })

  it('fetches the profile from the API and saves changes to the account', async () => {
    sessionStorage.setItem('authUser', JSON.stringify({ id: 1, username: 'jdoe' }))
    const wrapper = mount(Profile, { global: { stubs: globalStubs } })
    await flushPromises()

    expect(getProfile).toHaveBeenCalledTimes(1)
    expect((wrapper.get('#email').element).value).toBe('john@example.com')
    expect((wrapper.get('#fullname').element).value).toBe('John Doe')
    expect((wrapper.get('#bio').element).value).toBe('Consultant')

    await wrapper.get('#email').setValue('updated@example.com')
    await wrapper.get('#bio').setValue('Updated bio')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(updateProfile).toHaveBeenCalledWith({
      email: 'updated@example.com',
      fullName: 'John Doe',
      bio: 'Updated bio',
    })
    expect(wrapper.text()).toContain('Profile updated successfully!')
    expect(JSON.parse(sessionStorage.getItem('authUser')).email).toBe('updated@example.com')
  })

  it('shows registration form errors and persists a profile on success', async () => {
    const wrapper = mount(Register, { global: { stubs: globalStubs } })

    expect(wrapper.get('button[type="submit"]').attributes('disabled')).toBeUndefined()
    expect(wrapper.text()).toContain('Use at least 12 characters.')

    await wrapper.get('#password').setValue('secret123')
    await wrapper.get('#confirm').setValue('different123')
    await wrapper.get('form').trigger('submit')
    expect(wrapper.text()).toContain('Passwords do not match')

    await wrapper.get('#fullname').setValue('Jane Doe')
    await wrapper.get('#username').setValue('janedoe')
    await wrapper.get('#email').setValue('jane@example.com')
    await wrapper.get('#password').setValue('secret-password')
    await wrapper.get('#confirm').setValue('secret-password')
    await wrapper.get('form').trigger('submit')

    await flushPromises()

    const user = JSON.parse(sessionStorage.getItem('authUser'))
    expect(user.username).toBe('janedoe')
    expect(routerPushMock).toHaveBeenCalledWith('/dashboard')
  })

  it('loads the service list from the API and includes AI Services', async () => {
    const wrapper = mount(Service, { global: { stubs: globalStubs } })

    await flushPromises()
    await wrapper.vm.$nextTick()

    expect(getServices).toHaveBeenCalledTimes(1)
    expect(wrapper.text()).toContain('Our Services')
    expect(wrapper.text()).toContain('AI Services')
  })

  it('renders the requested service detail view from the route', async () => {
    routeState.params.serviceId = 'custom-development'
    const wrapper = mount(ServiceDetail, { global: { stubs: globalStubs } })

    expect(wrapper.text()).toContain('Custom Development')
    expect(wrapper.text()).toContain('Full-stack web and mobile applications')
  })

  it('falls back to the first service when a route does not match', async () => {
    routeState.params.serviceId = 'missing-service'
    const wrapper = mount(ServiceDetail, { global: { stubs: globalStubs } })

    expect(wrapper.text()).toContain('Strategy & Transformation')
  })

  it('redirects to the services index when the requested service is missing', async () => {
    routeState.params.serviceId = 'missing-service'
    vi.mocked(getServiceById).mockResolvedValueOnce(null)

    const wrapper = mount(ServiceDetail, { global: { stubs: globalStubs } })
    await flushPromises()

    expect(routerReplaceMock).toHaveBeenCalledWith('/services')
    expect(wrapper.text()).toContain('Service not found')
  })

  it('validates billing submissions and marks invoices paid', async () => {
    const wrapper = mount(BillingOptions, { global: { stubs: globalStubs } })
    await wrapper.vm.$nextTick()

    await wrapper.get('.btn-pay').trigger('click')
    await wrapper.get('form').trigger('submit')
    expect(wrapper.text()).toContain('Please fill in all required credit card fields')

    await wrapper.get('#cardName').setValue('Jane Doe')
    await wrapper.get('#cardNumber').setValue('4111111111111111')
    await wrapper.get('#expiry').setValue('12/30')
    await wrapper.get('#cvv').setValue('123')
    await wrapper.get('form').trigger('submit')

    vi.advanceTimersByTime(1500)
    await wrapper.vm.$nextTick()
    await Promise.resolve()

    expect(wrapper.text()).toContain('processed successfully')
    const invoices = JSON.parse(localStorage.getItem('userInvoices'))
    expect(invoices[0].status).toBe('Paid')
  })
})

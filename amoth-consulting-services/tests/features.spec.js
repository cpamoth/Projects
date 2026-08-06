import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'

const routerPushMock = vi.fn()
const routerReplaceMock = vi.fn()
const routeState = { params: { serviceId: 'strategy-transformation' }, path: '/' }

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
import ServiceDetail from '../src/features/services/components/ServiceDetail.vue'
import BillingOptions from '../src/features/profile/components/BillingOptions.vue'

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
    routerPushMock.mockReset()
    routerReplaceMock.mockReset()
    routeState.params.serviceId = 'strategy-transformation'
    routeState.path = '/'
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
    await wrapper.get('#password').setValue('secret123')
    await wrapper.get('form').trigger('submit')

    vi.advanceTimersByTime(300)

    expect(wrapper.emitted('login')).toBeTruthy()
    expect(localStorage.getItem('token')).toBeTruthy()
    expect(routerPushMock).toHaveBeenCalledWith('/dashboard')
  })

  it('shows registration form errors and persists a profile on success', async () => {
    const wrapper = mount(Register, { global: { stubs: globalStubs } })

    await wrapper.get('#password').setValue('secret123')
    await wrapper.get('#confirm').setValue('different123')
    await wrapper.get('form').trigger('submit')
    expect(wrapper.text()).toContain('Passwords do not match')

    await wrapper.get('#fullname').setValue('Jane Doe')
    await wrapper.get('#username').setValue('janedoe')
    await wrapper.get('#password').setValue('secret123')
    await wrapper.get('#confirm').setValue('secret123')
    await wrapper.get('form').trigger('submit')

    vi.advanceTimersByTime(600)

    const profile = JSON.parse(localStorage.getItem('userProfile'))
    expect(localStorage.getItem('token')).toBeTruthy()
    expect(profile.fullName).toBe('Jane Doe')
    expect(routerPushMock).toHaveBeenCalledWith('/dashboard')
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

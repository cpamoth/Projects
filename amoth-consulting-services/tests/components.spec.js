import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/vue'
import About from '../src/components/About.vue'
import Accessibility from '../src/components/Accessibility.vue'
import ActiveProjects from '../src/components/ActiveProjects.vue'
import BillingOptions from '../src/components/BillingOptions.vue'
import CompletionRate from '../src/components/CompletionRate.vue'
import Dashboard from '../src/components/Dashboard.vue'
import FullProjectDashboard from '../src/components/FullProjectDashboard.vue'
import Home from '../src/components/Home.vue'
import Login from '../src/components/Login.vue'
import NavBar from '../src/components/NavBar.vue'
import PageWrapper from '../src/components/PageWrapper.vue'
import PendingTasks from '../src/components/PendingTasks.vue'
import Profile from '../src/components/Profile.vue'
import Register from '../src/components/Register.vue'
import ScheduledMeetings from '../src/components/ScheduledMeetings.vue'
import Service from '../src/components/Service.vue'
import ServiceDetail from '../src/components/ServiceDetail.vue'
import TheWelcome from '../src/components/TheWelcome.vue'
import WelcomeItem from '../src/components/WelcomeItem.vue'

const globalStubs = {
  PageWrapper: {
    template: '<div><slot /></div>',
  },
  BillingOptions: true,
  ActiveProjects: true,
  PendingTasks: true,
  ScheduledMeetings: true,
  CompletionRate: true,
  WelcomeItem: true,
  RouterLink: {
    props: ['to'],
    template: '<a><slot /></a>',
  },
}

describe('Vue component smoke tests', () => {
  it('renders About.vue', async () => {
    render(About, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Our Experience/i)).toBeTruthy()
  })

  it('renders Accessibility.vue', async () => {
    render(Accessibility, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Accessibility Statement/i)).toBeTruthy()
  })

  it('renders ActiveProjects.vue', async () => {
    render(ActiveProjects, { global: { stubs: globalStubs }, props: { showHeader: true } })
    expect(await screen.findByText(/Active Projects/i)).toBeTruthy()
  })

  it('renders BillingOptions.vue', async () => {
    render(BillingOptions, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Outstanding Invoices/i)).toBeTruthy()
  })

  it('renders CompletionRate.vue', async () => {
    render(CompletionRate, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Completion Rate/i)).toBeTruthy()
  })

  it('renders Dashboard.vue', async () => {
    render(Dashboard, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Welcome back/i)).toBeTruthy()
  })

  it('renders FullProjectDashboard.vue', async () => {
    render(FullProjectDashboard, { global: { stubs: globalStubs } })
    const headings = await screen.findAllByRole('heading', { name: /Full Project Dashboard/i })
    expect(headings.length).toBeGreaterThan(0)
  })

  it('renders Home.vue', async () => {
    render(Home, { props: { msg: 'Test message' }, global: { stubs: globalStubs } })
    expect(await screen.findByText(/Test message/i)).toBeTruthy()
  })

  it('renders Login.vue', async () => {
    render(Login, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Account Login/i)).toBeTruthy()
  })

  it('renders NavBar.vue', async () => {
    render(NavBar, { global: { stubs: globalStubs } })
    expect(await screen.findByRole('link', { name: /Amoth Consulting home/i })).toBeTruthy()
  })

  it('renders PageWrapper.vue', async () => {
    render(PageWrapper, {
      global: { stubs: globalStubs },
      slots: { default: '<div data-testid="page-wrapper-slot">slot content</div>' },
    })
    expect(screen.getByTestId('page-wrapper-slot')).toBeTruthy()
  })

  it('renders PendingTasks.vue', async () => {
    render(PendingTasks, { global: { stubs: globalStubs } })
    const headings = await screen.findAllByText(/Pending Tasks/i)
    expect(headings.length).toBeGreaterThan(0)
  })

  it('renders Profile.vue', async () => {
    render(Profile, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/My Profile/i)).toBeTruthy()
  })

  it('renders Register.vue', async () => {
    render(Register, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Sign Up for New Account/i)).toBeTruthy()
  })

  it('renders ScheduledMeetings.vue', async () => {
    render(ScheduledMeetings, { global: { stubs: globalStubs } })
    const headings = await screen.findAllByText(/Scheduled Meetings/i)
    expect(headings.length).toBeGreaterThan(0)
  })

  it('renders Service.vue', async () => {
    render(Service, { global: { stubs: globalStubs } })
    expect(await screen.findByText(/Our Services/i)).toBeTruthy()
  })

  it('renders ServiceDetail.vue', async () => {
    render(ServiceDetail, { global: { stubs: globalStubs } })
    const matches = await screen.findAllByText(/Strategy & Transformation/i)
    expect(matches.length).toBeGreaterThan(0)
  })

  it('renders TheWelcome.vue', async () => {
    render(TheWelcome, {
      global: { stubs: globalStubs },
      slots: { default: '<p>Welcome to the platform</p>' },
    })
    expect(await screen.findByText(/Welcome to the platform/i)).toBeTruthy()
  })

  it('renders WelcomeItem.vue', async () => {
    render(WelcomeItem, { global: { stubs: globalStubs }, slots: { default: '<span>item</span>' } })
    expect(await screen.findByText(/item/i)).toBeTruthy()
  })
})

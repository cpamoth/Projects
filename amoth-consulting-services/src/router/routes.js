import HomeView from '../features/dashboard/views/HomeView.vue'
import AboutView from '../features/content/views/AboutView.vue'
import LoginView from '../features/auth/views/LoginView.vue'
import DashboardView from '../features/dashboard/views/DashboardView.vue'
import RegisterView from '../features/auth/views/RegisterView.vue'
import ServicesView from '../features/services/views/ServicesView.vue'
import ServiceDetailView from '../features/services/views/ServiceDetailView.vue'
import ProfileView from '../features/profile/views/ProfileView.vue'
import ProjectsView from '../features/projects/views/ProjectsView.vue'
import AccessibilityView from '../features/content/views/AccessibilityView.vue'

export const routes = [
  { path: '/', component: HomeView },
  { path: '/about', component: AboutView },
  { path: '/accessibility', component: AccessibilityView },
  { path: '/services', component: ServicesView },
  { path: '/services/:serviceId', component: ServiceDetailView },
  { path: '/login', component: LoginView },
  { path: '/dashboard', component: DashboardView, meta: { requiresAuth: true } },
  { path: '/projects', component: ProjectsView, meta: { requiresAuth: true } },
  { path: '/profile', component: ProfileView, meta: { requiresAuth: true } },
  { path: '/register', component: RegisterView }
]

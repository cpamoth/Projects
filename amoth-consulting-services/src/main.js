import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHistory } from 'vue-router'
import Home from './components/Home.vue'
import About from './components/About.vue'
import Login from './components/Login.vue'
import Dashboard from './components/Dashboard.vue'
import Register from './components/Register.vue'
import Service from './components/Service.vue'
import ServiceDetail from './components/ServiceDetail.vue'
import Profile from './components/Profile.vue'
import FullProjectDashboard from './components/FullProjectDashboard.vue'

const routes = [
  { path: '/', component: Home },
  { path: '/about', component: About },
  { path: '/services', component: Service },
  { path: '/services/:serviceId', component: ServiceDetail },
  { path: '/login', component: Login }
  ,{ path: '/dashboard', component: Dashboard, meta: { requiresAuth: true } }  ,{ path: '/projects', component: FullProjectDashboard, meta: { requiresAuth: true } }  ,{ path: '/profile', component: Profile, meta: { requiresAuth: true } }
  ,{ path: '/register', component: Register }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// global guard for protected routes
router.beforeEach((to, from, next) => {
  if (to.meta && to.meta.requiresAuth) {
    const token = localStorage.getItem('token')
    if (!token) return next('/login')
  }
  next()
})

import PageWrapper from './layouts/PageWrapper.vue'

const app = createApp(App)
app.component('PageWrapper', PageWrapper)
app.use(router).mount('#app')

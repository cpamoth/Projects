import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './router/routes.js'

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

import PageWrapper from './shared/layouts/PageWrapper.vue'

const app = createApp(App)
app.component('PageWrapper', PageWrapper)
app.use(router).mount('#app')

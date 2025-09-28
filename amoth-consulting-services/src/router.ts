import { createRouter, createWebHistory } from 'vue-router';
import Home from './views/Home.vue';
import About from './views/About.vue';
import LoginForm from './components/LoginForm.vue';

const routes = [
  { path: '/', component: Home },
  { path: '/about', component: About },
  { path: '/login', component: LoginForm },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
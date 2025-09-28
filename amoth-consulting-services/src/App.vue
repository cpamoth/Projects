<script setup lang="ts">
import Home from './components/Home.vue'
import NavBar from './components/NavBar.vue';
import LoginForm from './components/LoginForm.vue';

// Import your authentication store (adjust the path as needed)
import { useAuthStore } from './stores/authStore';

// Initialize the store
const authStore = useAuthStore();
</script>

<template>
  <div id="app">
      <template v-if="!authStore.isAuthenticated">
          <LoginForm />
      </template>
      <template v-else>
          <p>Welcome, {{ authStore.user.username }}!</p>
          <button @click="authStore.logout">Logout</button>
          <!-- Your protected content here -->
      </template>
  </div>
  <NavBar />
  <router-view />
  <div>
  </div>
  <Home msg="Amoth Consulting Services" />
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>

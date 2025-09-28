<!-- src/components/Navbar.vue -->
<template>
  <header class="navbar">
    <nav>
      <RouterLink to="/">Home</RouterLink>
      <RouterLink to="/about">About</RouterLink>
      <RouterLink to="/dashboard" v-if="isAuthenticated">Dashboard</RouterLink>
      <button v-if="isAuthenticated" @click="logout">Logout</button>
      <RouterLink to="/login" v-else>Login</RouterLink>
    </nav>
  </header>
</template>

<script setup>  
import { RouterLink } from 'vue-router';

function logout() {
  userStore.logout(); // Call an action from your Pinia store
  // Redirect to login or home page
}
</script>

<style scoped>
.navbar {
  background-color: #000;
  padding: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

nav a {
  margin-right: 15px;
  text-decoration: none;
  color: #ffffff;
}
</style>
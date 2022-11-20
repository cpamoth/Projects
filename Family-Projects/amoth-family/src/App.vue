<template>
  <div id="app">
    <NavigationMenu
    :navLinks="navLinks"
    />
  <router-view/>
  </div>
</template>

<script>
import Home from './components/Home.vue'
import NavigationMenu from './components/NavigationMenu.vue'

export default {
  name: 'App',
  components: {
    Home,
    NavigationMenu
  },
  data: () => ({
    navLinks: [
      {
        text: 'Contact',
        path: '/contact'
      },
      {
        text: 'About',
        path: '/about'
      },
      {
        text: 'Login',
        path: '/login'
      }
    ]
  })
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>

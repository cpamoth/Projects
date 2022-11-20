<template>
  <app-header />
  <img alt="" src="">
  <AmothFamily msg="Welcome to the Amoth US Site"/>
  <app-footer />
</template>

<script>
import AmothFamily from './components/AmothFamily.vue'
import AppHeader from '@/components/AppHeader.vue';
import AppFooter from '@/components/AppFooter.vue';

export default {
  name: 'App',
  components: {
    AmothFamily,
    AppHeader,
    AppFooter
  }
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

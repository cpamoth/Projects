import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './router'
//import { Pinia } from 'pinia'

//const pinia = Pinia();

createApp(App).use(router).mount('#app')

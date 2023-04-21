import { createApp } from 'vue'
import './style.css'
import { createRouter, createWebHashHistory }from 'vue-router';
import HomePage from './components/home/HomePage.vue';
import App from './App.vue'

const routes = [
    { path: '/', name: 'home', component: HomePage },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
})
const app = createApp({});

app.use(router);

app.mount('#app')

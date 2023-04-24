import { createWebHashHistory, createRouter } from 'vue-router'
import HomePage from '../components/home/HomePage.vue'
import AboutPage from '../components/about/AboutPage.vue'

const routes = [
    {
        path: '/',
        name: 'home',
        component: HomePage
    },
    {
        path: '/about',
        name: 'about',
        component: AboutPage
    },
    {
        path: '/game/:slug',
        name: 'game',
        props: true,
        component: () => import('../components/game/GamePage.vue')
    }
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;
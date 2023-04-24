<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import * as L from 'leaflet';
import { onMounted, ref } from 'vue';

const map = ref<L.Map>();

const emit = defineEmits<{
    (e: 'click', latLng: L.LatLng): void
}>()

const onMapClick = (e) => {
    emit('click', e.latlng);
}

onMounted(() => {
    map.value = L.map('map').setView([51.505, -0.09], 13);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map.value);
    map.value.on('click', onMapClick);
})

</script>

<template>
    <div style="height: 600px; width: 800px">
        <div id="map" class="w-full h-full"></div>
    </div>
</template>
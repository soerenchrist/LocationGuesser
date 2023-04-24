<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import * as L from 'leaflet';
import { onMounted, ref, watch } from 'vue';

const map = ref<L.Map>();
const marker = ref<L.Marker>();

const emit = defineEmits<{
    (e: 'click', latLng: L.LatLng): void
}>()

const props = defineProps<{
    position?: L.LatLng,
}>();

const onMapClick = (e: any) => {
    emit('click', e.latlng);
}

watch(props, (newProps) => {
    if (!newProps.position) {
        if (marker.value) {
            map.value!.removeLayer(marker.value);
            marker.value = undefined;
        }

        return;
    }
    if (!marker.value) {
        marker.value = L.marker(newProps.position).addTo(map.value!);
    }
    marker.value.setLatLng(newProps.position);
});

onMounted(() => {
    map.value = L.map('map').setView([45.505, -0], 3);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map.value);
    map.value.on('click', onMapClick);
})

</script>

<template>
    <div style="height: 600px; width: 100%">
        <div id="map" class="w-full h-full"></div>
    </div>
</template>
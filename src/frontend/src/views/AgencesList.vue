<template>
  <div class="agences-list">
    <h1>Agences</h1>
    <div class="list">
      <div v-for="agence in agences" 
           :key="agence" 
           class="list-item"
           @click="goToMagasins(agence)">
        {{ agence }}
      </div>
    </div>
    <div v-if="loading" class="loading">
      Chargement...
    </div>
    <div v-if="error" class="error">
      {{ error }}
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { API_URL } from '../config'

export default {
  name: 'AgencesList',
  setup() {
    const agences = ref([])
    const router = useRouter()
    const loading = ref(false)
    const error = ref(null)

    const fetchAgences = async () => {
      loading.value = true
      error.value = null
      try {
        const response = await fetch(`${API_URL}/api/Agences`)
        if (!response.ok) throw new Error('Erreur lors de la récupération des agences')
        agences.value = await response.json()
      } catch (e) {
        error.value = e.message
        console.error('Erreur:', e)
      } finally {
        loading.value = false
      }
    }

    const goToMagasins = (agence) => {
      router.push({
        name: 'magasins',
        params: {
          agence: agence,
          agenceName: agence
        }
      })
    }

    onMounted(fetchAgences)

    return {
      agences,
      loading,
      error,
      goToMagasins
    }
  }
}
</script>

<style scoped>
.agences-list {
  padding: 1rem;
  max-width: 600px;
  margin: 0 auto;
}

h1 {
  text-align: center;
  color: #2c3e50;
  margin-bottom: 2rem;
}

.list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.list-item {
  background: white;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.list-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.list-item:active {
  transform: translateY(0);
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.error {
  color: #dc3545;
  text-align: center;
  padding: 1rem;
  margin-top: 1rem;
}
</style>

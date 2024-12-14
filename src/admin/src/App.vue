<template>
  <v-app>
    <v-app-bar :color="colors.primary" dark>
      <v-img :src="logoSvg" max-height="40" max-width="40" contain class="mr-3" />
      <v-toolbar-title>Administration Inventaire</v-toolbar-title>
      <v-spacer></v-spacer>
    </v-app-bar>

    <v-main>
      <v-container>
        <v-row>
          <!-- Colonne de téléchargement -->
          <v-col cols="6">
            <v-card class="pa-4" elevation="2">
              <v-card-title class="text-h6">Télécharger l'inventaire</v-card-title>
              <v-card-text>
                Téléchargez le fichier CSV actuel contenant l'inventaire complet des équipements.
              </v-card-text>
              <v-card-actions>
                <v-btn
                  :color="colors.primary"
                  @click="downloadCsv"
                  :loading="downloading"
                  prepend-icon="mdi-download"
                >
                  Télécharger le CSV
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-col>

          <!-- Colonne d'upload -->
          <v-col cols="6">
            <v-card class="pa-4" elevation="2">
              <v-card-title class="text-h6">Mettre à jour l'inventaire</v-card-title>
              <v-card-text>
                Importez un nouveau fichier CSV pour mettre à jour l'inventaire.
                <br><br>
                <strong>Note importante:</strong> Le fichier doit respecter le format suivant:
                <ul>
                  <li>Format CSV avec séparateur point-virgule (;)</li>
                  <li>Colonnes requises: ID, Nom, Description, etc.</li>
                </ul>
              </v-card-text>
              <v-card-actions>
                <v-file-input
                  v-model="fileToUpload"
                  accept=".csv"
                  label="Sélectionner un fichier CSV"
                  prepend-icon="mdi-file-upload"
                  :loading="uploading"
                  class="mb-4"
                ></v-file-input>
              </v-card-actions>
              <v-card-actions>
                <v-btn
                  :color="colors.primary"
                  @click="uploadCsv"
                  :disabled="!fileToUpload"
                  :loading="uploading"
                  prepend-icon="mdi-cloud-upload"
                >
                  Importer le fichier
                </v-btn>
              </v-card-actions>
            </v-card>

            <!-- Bouton de suppression -->
            <v-card class="mt-4 pa-4" elevation="2">
              <v-card-title class="text-h6">Zone de danger</v-card-title>
              <v-card-text>
                Attention: Cette action supprimera définitivement le fichier d'inventaire actuel.
              </v-card-text>
              <v-card-actions>
                <v-btn
                  :color="colors.warning"
                  @click="confirmDelete"
                  :loading="deleting"
                  prepend-icon="mdi-delete-alert"
                  variant="elevated"
                >
                  Supprimer l'inventaire
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>

    <!-- Dialog de confirmation pour la suppression -->
    <v-dialog v-model="deleteDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirmation de suppression</v-card-title>
        <v-card-text>
          Êtes-vous sûr de vouloir supprimer le fichier d'inventaire ? Cette action est irréversible.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey" variant="text" @click="deleteDialog = false">Annuler</v-btn>
          <v-btn :color="colors.warning" variant="elevated" @click="deleteCsv">Confirmer la suppression</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar pour les messages -->
    <v-snackbar v-model="snackbar.show" :color="snackbar.color">
      {{ snackbar.text }}
      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar.show = false">Fermer</v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const colors = {
  primary: '#0b71b3',  // bleu
  secondary: '#a4c307', // vert
  warning: '#ec9001'   // orange
}

const logoSvg = `<?xml version="1.0" encoding="UTF-8"?> <svg xmlns="http://www.w3.org/2000/svg" width="156.314" height="67.396" viewBox="0 0 156.314 67.396"><g transform="translate(0.254 28.243)"><path d="M70.351,67.948c-18.653,10.75-26.69-2.1-34.7-2.1a14.091,14.091,0,0,0-9.609,3.809c.526,4.545,3.714,9.33,8.817,12.461a18.552,18.552,0,0,0,13.481,2.687,19.5,19.5,0,0,1-10.534.627c-8.005-1.648-13.779-7.466-13.767-13.5a15.4,15.4,0,0,0-2.884,8.994,15.941,15.941,0,0,0,6.186,12.48c13.539,10.991,44.792,3.7,43.005-25.461" transform="translate(-21.16 -59.119)" fill="#a4c307"></path><path d="M125.472,60.127a6.2,6.2,0,0,0-5.026,2.237,8.968,8.968,0,0,0-1.863,5.926,8.783,8.783,0,0,0,1.863,5.914,6.108,6.108,0,0,0,4.874,2.18,6.209,6.209,0,0,0,4.937-2.117,8.716,8.716,0,0,0,1.819-5.876,9.4,9.4,0,0,0-1.768-6.091,5.9,5.9,0,0,0-4.836-2.174m-.266,21.157a12.826,12.826,0,0,1-9.343-3.518,12.274,12.274,0,0,1-3.613-9.178A12.629,12.629,0,0,1,125.63,55.24,12.123,12.123,0,0,1,138.4,68.081a12.52,12.52,0,0,1-13.2,13.2" transform="translate(-54.515 -55.234)" fill="#0b71b3"></path><path d="M178.678,79.943a18.585,18.585,0,0,1-7.631,1.331q-6.151,0-9.66-3.41a12.045,12.045,0,0,1-3.518-9.076,12.893,12.893,0,0,1,3.955-9.8,14.3,14.3,0,0,1,10.268-3.759,20.257,20.257,0,0,1,6.592.932v5.457a12.216,12.216,0,0,0-6.11-1.509,8.367,8.367,0,0,0-6.066,2.225,8.014,8.014,0,0,0-2.307,6.04,7.481,7.481,0,0,0,8.037,7.986,13.117,13.117,0,0,0,6.446-1.616v5.2Z" transform="translate(-71.22 -55.23)" fill="#0b71b3"></path></g><path d="M214.05,81.092H198.04V55.91h15.4v4.621h-9.387v5.6h8.734v4.6h-8.734V76.49h10Z" transform="translate(-85.675 -27.236)" fill="#0b71b3"></path><path d="M242.327,71.134l-2.871-8.462a10.287,10.287,0,0,1-.444-2.263h-.146A9.276,9.276,0,0,1,238.4,62.6l-2.909,8.531Zm9.831,9.957H245.6l-1.9-5.6h-9.495l-1.882,5.6H225.81l9.716-25.182h7.131Z" transform="translate(-95.844 -27.236)" fill="#0b71b3"></path><path d="M84.6,59.246V45.41l2,1.147v4.792l.843.666,3.106,2.472.025,2.744c.723-.323,1.464-.685,2.218-1.071L92.781,53.4l-3.936-3.137v-5L82.38,41.55V59.658c.723-.1,1.464-.235,2.225-.412" transform="translate(-43.324 -21.977)" fill="#ec9001"></path><path d="M64.588,50.556V21.438h6.18v29.98a26.6,26.6,0,0,0,3.125-.158V18.32H61.47V49.587c1,.355,2.035.691,3.118.97" transform="translate(-35.667 -13.471)" fill="#0b71b3"></path><path d="M41.941,42.678a10.375,10.375,0,0,1,2.18.254V26.877l4.19-2.44,1.109-.647V15.031l1.908-.83V45.613c.723.3,1.464.6,2.231.894V10.67l-.913.526-5.457,2.377v8.943l-5.3,3.08V42.678Z" transform="translate(-27.702 -10.67)" fill="#0b71b3"></path></svg>`

const fileToUpload = ref(null)
const uploading = ref(false)
const downloading = ref(false)
const deleting = ref(false)
const deleteDialog = ref(false)
const snackbar = ref({
  show: false,
  text: '',
  color: 'success'
})

const showMessage = (text, type = 'success') => {
  snackbar.value = {
    show: true,
    text,
    color: type
  }
}

const downloadCsv = async () => {
  try {
    downloading.value = true
    const response = await axios.get('/api/admin/csv', { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', 'inventory.csv')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    showMessage('Fichier téléchargé avec succès')
  } catch (error) {
    showMessage('Erreur lors du téléchargement du fichier', 'error')
  } finally {
    downloading.value = false
  }
}

const uploadCsv = async () => {
  if (!fileToUpload.value) return

  const formData = new FormData()
  formData.append('file', fileToUpload.value)

  try {
    uploading.value = true
    await axios.post('/api/admin/csv', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    showMessage('Fichier uploadé avec succès')
  } catch (error) {
    showMessage('Erreur lors de l\'upload du fichier', 'error')
  } finally {
    uploading.value = false
    fileToUpload.value = null
  }
}

const confirmDelete = () => {
  deleteDialog.value = true
}

const deleteCsv = async () => {
  try {
    deleting.value = true
    await axios.delete('/api/admin/csv')
    showMessage('Fichier supprimé avec succès')
    deleteDialog.value = false
  } catch (error) {
    showMessage('Erreur lors de la suppression du fichier', 'error')
  } finally {
    deleting.value = false
  }
}
</script>

<style>
.v-application {
  background-color: #f5f5f5;
}
</style>

import axios from 'axios'

const BASE_URL = 'http://localhost:7001'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' }
})

// Tự động thêm token vào mọi request
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken')
  if (token)
    config.headers.Authorization = `Bearer ${token}`
  return config
})

// Tự động refresh token khi 401
apiClient.interceptors.response.use(
  res => res,
  async err => {
    const original = err.config

    if (err.response?.status === 401 && !original._retry) {
      original._retry = true

      try {
        const userId       = localStorage.getItem('userId')
        const refreshToken = localStorage.getItem('refreshToken')

        const { data } = await axios.post(
          `${BASE_URL}/api/v1/auth/refresh`,
          { userId, refreshToken })

        localStorage.setItem('accessToken', data.accessToken)
        localStorage.setItem('refreshToken', data.refreshToken)

        original.headers.Authorization = `Bearer ${data.accessToken}`
        return apiClient(original)
      } catch {
        // Refresh fail → logout
        localStorage.clear()
        window.location.href = '/login'
      }
    }

    return Promise.reject(err)
  }
)
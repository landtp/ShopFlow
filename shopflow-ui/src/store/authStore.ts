import { create } from 'zustand'

// Cài zustand
// npm install zustand

interface AuthState {
  userId: string | null
  email: string | null
  firstName: string | null
  token: string | null
  isAuthenticated: boolean
  login: (data: {
    userId: string
    email: string
    firstName: string
    accessToken: string
    refreshToken: string
  }) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>(set => ({
  userId:          localStorage.getItem('userId'),
  email:           localStorage.getItem('email'),
  firstName:       localStorage.getItem('firstName'),
  token:           localStorage.getItem('accessToken'),
  isAuthenticated: !!localStorage.getItem('accessToken'),

  login: data => {
    localStorage.setItem('userId',        data.userId)
    localStorage.setItem('email',         data.email)
    localStorage.setItem('firstName',     data.firstName)
    localStorage.setItem('accessToken',   data.accessToken)
    localStorage.setItem('refreshToken',  data.refreshToken)

    set({
      userId:          data.userId,
      email:           data.email,
      firstName:       data.firstName,
      token:           data.accessToken,
      isAuthenticated: true
    })
  },

  logout: () => {
    localStorage.clear()
    set({
      userId: null, email: null,
      firstName: null, token: null,
      isAuthenticated: false
    })
  }
}))
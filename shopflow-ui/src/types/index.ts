export interface AuthResponse {
  userId: string
  email: string
  firstName: string
  lastName: string
  role: string
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
}

export interface OrderItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  subTotal: number
}

export interface Order {
  id: string
  customerId: string
  status: string
  totalAmount: number
  currency: string
  shippingAddress: string
  createdAt: string
  updatedAt: string | null
  items: OrderItem[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
}

export interface CreateOrderRequest {
  customerId: string
  shippingAddress: string
  items: {
    productId: string
    productName: string
    quantity: number
    unitPrice: number
    currency: string
  }[]
}
// src/api/orders.ts
import { apiClient } from './client'
import  type  { CreateOrderRequest, Order, PagedResult } from '../types'

export const ordersApi = {
  getOrders: (customerId?: string, page = 1) =>
    apiClient.get<PagedResult<Order>>('/api/v1/orders', {
      params: { customerId, page, pageSize: 10 }
    }),

  getOrderById: (id: string) =>
    apiClient.get<Order>(`/api/v1/orders/${id}`),

  createOrder: (data: CreateOrderRequest) =>
    apiClient.post<{ orderId: string }>('/api/v1/orders', data),

  cancelOrder: (id: string, reason: string) =>
    apiClient.delete(`/api/v1/orders/${id}`, {
      data: { reason }
    })
}
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { message } from 'antd'
import { userService, type UserFilters, type CreateUserRequest, type UpdateUserRequest } from '../../services/userService'

// Query keys
export const userKeys = {
  all: ['users'] as const,
  lists: () => [...userKeys.all, 'list'] as const,
  list: (filters?: UserFilters) => [...userKeys.lists(), filters] as const,
  details: () => [...userKeys.all, 'detail'] as const,
  detail: (id: number) => [...userKeys.details(), id] as const,
}

// Get all users with filtering
export const useUsers = (filters?: UserFilters) => {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => userService.getUsers(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Get specific user by ID
export const useUser = (id: number) => {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => userService.getUser(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Create user mutation
export const useCreateUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (user: CreateUserRequest) => userService.createUser(user),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      message.success('User created successfully')
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to create user'
      message.error(errorMessage)
    },
  })
}

// Update user mutation
export const useUpdateUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, user }: { id: number; user: UpdateUserRequest }) => 
      userService.updateUser(id, user),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(userKeys.detail(variables.id), data)
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      message.success('User updated successfully')
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to update user'
      message.error(errorMessage)
    },
  })
}

// Delete user mutation
export const useDeleteUser = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => userService.deleteUser(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: userKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      message.success('User deleted successfully')
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to delete user'
      message.error(errorMessage)
    },
  })
}

// Assign clinic access mutation
export const useAssignBranchAccess = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, BranchIds }: { userId: number; BranchIds: number[] }) => 
      userService.AssignBranchAccess(userId, BranchIds),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: userKeys.detail(variables.userId) })
      queryClient.invalidateQueries({ queryKey: userKeys.lists() })
      message.success('Clinic access updated successfully')
    },
    onError: (error: any) => {
      const errorMessage = error.response?.data?.errors?.[0] || error.response?.data?.message || 'Failed to update clinic access'
      message.error(errorMessage)
    },
  })
}


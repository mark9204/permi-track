import type { Role } from '../../roles/services/roleService';
import { useEffect } from 'react';
import { Drawer, Form, Input, Button, Select, message, Space } from 'antd';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { userService } from '../services/userService';
import type { CreateUserRequest, UpdateUserRequest } from '../services/userService';
import roleService from '../../roles/services/roleService';
import type { User } from '../../../types/auth.types';
import { useUserPermissions } from '../../../hooks/useUserPermissions';

interface UserDrawerProps {
  open: boolean;
  onClose: () => void;
  userToEdit: User | null;
}

const UserDrawer = ({ open, onClose, userToEdit }: UserDrawerProps) => {
  const { isSuperAdmin, userDepartment } = useUserPermissions();
  const [form] = Form.useForm();
  const queryClient = useQueryClient();
  const isEditMode = !!userToEdit;

  const { data: roles = [], isLoading: isRolesLoading } = useQuery<Role[]>({
    queryKey: ['roles'],
    queryFn: roleService.getAllRoles,
    select: (data) => {
      if (isSuperAdmin) return data;
      return data.filter(role => role.department === userDepartment);
    },
  });

  useEffect(() => {
    if (open) {
      if (userToEdit) {
        form.setFieldsValue({
          ...userToEdit,
          roleIds: [], // TODO: Map roles to IDs if available in user object, or fetch separately
        });
      } else {
        form.resetFields();
        // Pre-fill department for non-super admins
        if (!isSuperAdmin && userDepartment) {
          form.setFieldsValue({ department: userDepartment });
        }
      }
    }
  }, [open, userToEdit, form, isSuperAdmin, userDepartment]);

  const createMutation = useMutation({
    mutationFn: (data: CreateUserRequest) => userService.createUser(data),
    onSuccess: () => {
      message.success('User created successfully');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      onClose();
    },
    onError: () => {
      message.error('Failed to create user');
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateUserRequest) => userService.updateUser(userToEdit!.id, data),
    onSuccess: () => {
      message.success('User updated successfully');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      onClose();
    },
    onError: () => {
      message.error('Failed to update user');
    },
  });

  const onFinish = (values: any) => {
    if (isEditMode) {
      updateMutation.mutate(values);
    } else {
      createMutation.mutate(values);
    }
  };

  const isLoading = createMutation.isPending || updateMutation.isPending;

  return (
    <Drawer
      title={isEditMode ? 'Edit User' : 'Create User'}
      size={500}
      onClose={onClose}
      open={open}
      extra={
        <Space>
          <Button onClick={onClose}>Cancel</Button>
          <Button onClick={() => form.submit()} type="primary" loading={isLoading}>
            Submit
          </Button>
        </Space>
      }
    >
      <Form layout="vertical" form={form} onFinish={onFinish}>
        <Form.Item
          name="username"
          label="Username"
          rules={[{ required: true, message: 'Please enter username' }]}
        >
          <Input placeholder="jdoe" />
        </Form.Item>

        <Form.Item
          name="email"
          label="Email"
          rules={[{ required: true, type: 'email', message: 'Please enter a valid email' }]}
        >
          <Input placeholder="jdoe@example.com" />
        </Form.Item>

        <Form.Item
          name="firstName"
          label="First Name"
          rules={[{ required: true, message: 'Please enter first name' }]}
        >
          <Input placeholder="John" />
        </Form.Item>

        <Form.Item
          name="lastName"
          label="Last Name"
          rules={[{ required: true, message: 'Please enter last name' }]}
        >
          <Input placeholder="Doe" />
        </Form.Item>

        <Form.Item
          name="department"
          label="Department"
          rules={[{ required: true, message: 'Please enter department' }]}
        >
          <Input placeholder="IT" disabled={!isSuperAdmin} />
        </Form.Item>

        {!isEditMode && (
          <Form.Item
            name="password"
            label="Password"
            rules={[{ required: true, message: 'Please enter password' }]}
          >
            <Input.Password placeholder="Password" />
          </Form.Item>
        )}

        <Form.Item
          name="roleIds"
          label="Roles"
          rules={[{ required: true, message: 'Please select at least one role' }]}
        >
          <Select
            mode="multiple"
            placeholder="Select roles"
            options={roles.map((r: any) => ({ label: r.name, value: r.id }))}
            loading={isRolesLoading}
          />
        </Form.Item>
      </Form>
    </Drawer>
  );
};

export default UserDrawer;

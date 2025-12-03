import React, { useState } from 'react';
import { Table, Card, Button, Space, Popconfirm, Drawer, Form, Input, message, Typography, Select } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import permissionService from '../services/permissionService';
import roleService from '../../roles/services/roleService';
import systemConfigService from '../../admin/services/systemConfigService';
import type { Permission, CreatePermissionRequest } from '../services/permissionService';
import { useUserPermissions } from '../../../hooks/useUserPermissions';

const { Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

const PermissionListPage: React.FC = () => {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [editingPermission, setEditingPermission] = useState<Permission | null>(null);
  const [form] = Form.useForm();
  const queryClient = useQueryClient();
  const { isSuperAdmin, userDepartment } = useUserPermissions();

  // Fetch Permissions
  const { data: permissions, isLoading } = useQuery({
    queryKey: ['permissions'],
    queryFn: permissionService.getAllPermissions,
    select: (data) => {
      if (isSuperAdmin) return data;
      return data.filter(permission => permission.department === userDepartment);
    },
  });

  // Fetch Roles for the dropdown
  const { data: roles } = useQuery({
    queryKey: ['roles'],
    queryFn: roleService.getAllRoles,
    select: (data) => {
      if (isSuperAdmin) return data;
      return data.filter(role => role.department === userDepartment);
    },
  });

  const { data: departments = [], isLoading: isDepartmentsLoading } = useQuery({
    queryKey: ['departments'],
    queryFn: systemConfigService.getDepartments,
  });

  const { data: targetSystems = [] } = useQuery({
    queryKey: ['targetSystems'],
    queryFn: systemConfigService.getTargetSystems,
  });

  const { data: actions = [] } = useQuery({
    queryKey: ['actions'],
    queryFn: systemConfigService.getActions,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: async (data: CreatePermissionRequest & { roleId?: number }) => {
      const newPermission = await permissionService.createPermission({
        name: data.name,
        description: data.description
      });
      
      if (data.roleId) {
        await roleService.addPermissionToRole(data.roleId, newPermission.id);
      }
      
      return newPermission;
    },
    onSuccess: () => {
      message.success('Permission created successfully');
      handleCloseDrawer();
      queryClient.invalidateQueries({ queryKey: ['permissions'] });
    },
    onError: () => {
      message.error('Failed to create permission');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: CreatePermissionRequest }) => permissionService.updatePermission(id, data),
    onSuccess: () => {
      message.success('Permission updated successfully');
      handleCloseDrawer();
      queryClient.invalidateQueries({ queryKey: ['permissions'] });
    },
    onError: () => {
      message.error('Failed to update permission');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => permissionService.deletePermission(id),
    onSuccess: () => {
      message.success('Permission deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['permissions'] });
    },
    onError: () => {
      message.error('Failed to delete permission');
    },
  });

  // Drawer Handlers
  const handleAdd = () => {
    setEditingPermission(null);
    form.resetFields();
    if (!isSuperAdmin && userDepartment) {
      form.setFieldsValue({ department: userDepartment });
    }
    setIsDrawerOpen(true);
  };

  const handleEdit = (permission: Permission) => {
    setEditingPermission(permission);
    form.setFieldsValue({
      name: permission.name,
      description: permission.description,
      department: permission.department,
      targetSystem: permission.targetSystem,
      action: permission.action,
    });
    setIsDrawerOpen(true);
  };

  const handleCloseDrawer = () => {
    setIsDrawerOpen(false);
    setEditingPermission(null);
    form.resetFields();
  };

  const handleSubmit = (values: CreatePermissionRequest & { roleId?: number }) => {
    if (editingPermission) {
      updateMutation.mutate({ id: editingPermission.id as number, data: values });
    } else {
      createMutation.mutate(values);
    }
  };

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: Permission) => (
        <Space size="middle">
          <Button 
            icon={<EditOutlined />} 
            onClick={() => handleEdit(record)}
          />
          <Popconfirm
            title="Are you sure to delete this permission?"
            onConfirm={() => deleteMutation.mutate(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button icon={<DeleteOutlined />} danger />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className="permission-list-page">
      <Card 
        title="Permissions Management" 
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Add Permission
          </Button>
        }
      >
        <Table 
          columns={columns} 
          dataSource={permissions} 
          rowKey="id" 
          loading={isLoading}
        />
      </Card>

      <Drawer
        title={editingPermission ? "Edit Permission" : "Add New Permission"}
        width={500}
        onClose={handleCloseDrawer}
        open={isDrawerOpen}
        bodyStyle={{ paddingBottom: 80 }}
      >
        <Form layout="vertical" form={form} onFinish={handleSubmit}>
          <Form.Item
            name="targetSystem"
            label="Target System"
            rules={[{ required: true, message: 'Please select target system' }]}
          >
            <Select 
              placeholder="Select target system"
              onChange={(value) => {
                form.setFieldsValue({ action: undefined });
                const action = form.getFieldValue('action');
                if (value && action) {
                  form.setFieldsValue({ name: `${value}_${action}`.toUpperCase() });
                } else if (value) {
                   // If only system is selected, maybe just set that part or wait?
                   // Better to wait for action.
                }
              }}
            >
              {targetSystems.map((sys: any) => (
                <Option key={sys.id} value={sys.name}>{sys.name}</Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="action"
            label="Action"
            dependencies={['targetSystem']}
            rules={[{ required: true, message: 'Please select action' }]}
          >
            <Select 
              placeholder="Select action"
              onChange={(value) => {
                const system = form.getFieldValue('targetSystem');
                if (system && value) {
                  form.setFieldsValue({ name: `${system}_${value}`.toUpperCase() });
                }
              }}
            >
              {actions
                .filter((act: any) => {
                  const selectedSystemName = form.getFieldValue('targetSystem');
                  if (!selectedSystemName) return true;
                  const selectedSystem = targetSystems.find((s: any) => s.name === selectedSystemName);
                  return !selectedSystem || act.targetSystemId === selectedSystem.id;
                })
                .map((act: any) => (
                  <Option key={act.id} value={act.name}>{act.name}</Option>
                ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="name"
            label="Permission Name"
            rules={[{ required: true, message: 'Please enter permission name' }]}
          >
            <Input placeholder="e.g. READ_USERS" />
          </Form.Item>
          
          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <TextArea rows={4} placeholder="Describe what this permission allows..." />
          </Form.Item>

          <Form.Item
            name="department"
            label="Department"
            rules={[{ required: true, message: 'Please select department' }]}
          >
            <Select
              placeholder="Select department"
              disabled={!isSuperAdmin}
              loading={isDepartmentsLoading}
              options={departments.map(d => ({ label: d.name, value: d.name }))}
            />
          </Form.Item>

          {!editingPermission && (
            <Form.Item
              name="roleId"
              label="Add to Role (Optional)"
            >
              <Select placeholder="Select a role to assign this permission to" allowClear>
                {roles?.map((role: any) => (
                  <Option key={role.id} value={role.id}>{role.name}</Option>
                ))}
              </Select>
            </Form.Item>
          )}

          <Form.Item>
            <Space style={{ float: 'right' }}>
              <Button onClick={handleCloseDrawer}>Cancel</Button>
              <Button type="primary" htmlType="submit" loading={createMutation.isPending || updateMutation.isPending}>
                Submit
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Drawer>
    </div>
  );
};

export default PermissionListPage;

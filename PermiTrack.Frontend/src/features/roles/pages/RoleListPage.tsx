import React, { useState } from 'react';
import { Table, Card, Button, Space, Popconfirm, Drawer, Form, Input, message, Typography, Tag, Select, Checkbox, Divider } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import roleService from '../services/roleService';
import systemConfigService from '../../admin/services/systemConfigService';
import type { Role, CreateRoleRequest } from '../services/roleService';
import { useUserPermissions } from '../../../hooks/useUserPermissions';

const { Text } = Typography;
const { TextArea } = Input;

const RoleListPage: React.FC = () => {
  const { isSuperAdmin, userDepartment } = useUserPermissions();
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [form] = Form.useForm();
  const queryClient = useQueryClient();
  const selectedDepartmentName = Form.useWatch('department', form);

  // Fetch Roles
  const { data: roles, isLoading } = useQuery({
    queryKey: ['roles'],
    queryFn: roleService.getAllRoles,
    select: (data) => {
      if (isSuperAdmin) return data;
      // Filter by department for non-super admins
      return data.filter(r => r.department === userDepartment);
    }
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

  // Filter systems based on selected department
  const filteredSystems = React.useMemo(() => {
    if (!selectedDepartmentName) return [];
    const dept = departments.find(d => d.name === selectedDepartmentName);
    if (!dept) return [];
    return targetSystems.filter(sys => sys.departmentId === dept.id);
  }, [selectedDepartmentName, departments, targetSystems]);

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: CreateRoleRequest) => roleService.createRole(data),
    onSuccess: () => {
      message.success('Role created successfully');
      handleCloseDrawer();
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    },
    onError: () => {
      message.error('Failed to create role');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: CreateRoleRequest }) => roleService.updateRole(id, data),
    onSuccess: () => {
      message.success('Role updated successfully');
      handleCloseDrawer();
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    },
    onError: () => {
      message.error('Failed to update role');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => roleService.deleteRole(id),
    onSuccess: () => {
      message.success('Role deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    },
    onError: () => {
      message.error('Failed to delete role');
    },
  });

  // Drawer Handlers
  const handleAdd = () => {
    setEditingRole(null);
    form.resetFields();
    if (!isSuperAdmin && userDepartment) {
      form.setFieldsValue({ department: userDepartment });
    }
    setIsDrawerOpen(true);
  };

  const handleEdit = (role: Role) => {
    setEditingRole(role);
    form.setFieldsValue({
      name: role.name,
      description: role.description,
      department: role.department,
    });
    setIsDrawerOpen(true);
  };

  const handleCloseDrawer = () => {
    setIsDrawerOpen(false);
    setEditingRole(null);
    form.resetFields();
  };

  const handleSubmit = (values: CreateRoleRequest) => {
    if (editingRole) {
      updateMutation.mutate({ id: editingRole.id as number, data: values });
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
      render: (_: any, record: Role) => (
        <Space size="middle">
          <Button 
            type="text" 
            icon={<EditOutlined />} 
            onClick={() => handleEdit(record)} 
          />
          <Popconfirm
            title="Are you sure to delete this role?"
            onConfirm={() => deleteMutation.mutate(record.id as number)}
            okText="Yes"
            cancelText="No"
          >
            <Button type="text" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card 
        title="Role Management" 
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Add Role
          </Button>
        }
      >
        <Table
          rowKey="id"
          columns={columns}
          dataSource={roles || []}
          loading={isLoading}
          pagination={{ pageSize: 10 }}
        />
      </Card>

      <Drawer
        title={editingRole ? 'Edit Role' : 'Add Role'}
        width={500}
        onClose={handleCloseDrawer}
        open={isDrawerOpen}
        extra={
          <Space>
            <Button onClick={handleCloseDrawer}>Cancel</Button>
            <Button 
              type="primary" 
              onClick={() => form.submit()} 
              loading={createMutation.isPending || updateMutation.isPending}
            >
              Submit
            </Button>
          </Space>
        }
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            name="name"
            label="Role Name"
            rules={[{ required: true, message: 'Please enter role name' }]}
          >
            <Input placeholder="e.g. Manager" />
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
          >
            <TextArea rows={4} placeholder="Enter role description..." />
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

          {selectedDepartmentName && filteredSystems.length > 0 && (
            <>
              <Divider orientation="left">Permissions</Divider>
              {filteredSystems.map(sys => {
                const sysActions = actions.filter(a => a.targetSystemId === sys.id);
                if (sysActions.length === 0) return null;
                
                return (
                  <div key={sys.id} style={{ marginBottom: 16 }}>
                    <Text strong>{sys.name}</Text>
                    <div style={{ marginTop: 8 }}>
                      {sysActions.map(action => (
                        <Form.Item 
                          key={action.id} 
                          name={['permissions', `${sys.id}_${action.id}`]} 
                          valuePropName="checked"
                          noStyle
                        >
                          <Checkbox style={{ marginLeft: 8 }}>{action.name}</Checkbox>
                        </Form.Item>
                      ))}
                    </div>
                  </div>
                );
              })}
            </>
          )}
        </Form>
      </Drawer>
    </div>
  );
};

export default RoleListPage;

import React, { useState } from 'react';
import { Card, Table, Button, Input, Space, Popconfirm, message, Tabs, Form, Modal, Select, Tag } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import systemConfigService, { SYSTEM_CATEGORIES } from '../services/systemConfigService';
import type { SystemConfigOption } from '../services/systemConfigService';

const SystemConfigPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState('targetSystems');
  const [form] = Form.useForm();

  // Queries
  const { data: targetSystems = [], isLoading: isSystemsLoading } = useQuery({
    queryKey: ['targetSystems'],
    queryFn: systemConfigService.getTargetSystems,
  });

  const { data: actions = [], isLoading: isActionsLoading } = useQuery({
    queryKey: ['actions'],
    queryFn: systemConfigService.getActions,
  });

  const { data: departments = [], isLoading: isDepartmentsLoading } = useQuery({
    queryKey: ['departments'],
    queryFn: systemConfigService.getDepartments,
  });

  // Mutations
  const createSystemMutation = useMutation({
    mutationFn: (values: { name: string; category: string; departmentId?: number }) => 
      systemConfigService.createTargetSystem(values.name, values.category, values.departmentId),
    onSuccess: () => {
      message.success('Target System created');
      queryClient.invalidateQueries({ queryKey: ['targetSystems'] });
      handleCloseModal();
    },
  });

  const deleteSystemMutation = useMutation({
    mutationFn: (id: number) => systemConfigService.deleteTargetSystem(id),
    onSuccess: () => {
      message.success('Target System deleted');
      queryClient.invalidateQueries({ queryKey: ['targetSystems'] });
    },
  });

  const createActionMutation = useMutation({
    mutationFn: (values: { name: string; targetSystemId: number }) => 
      systemConfigService.createAction(values.name, values.targetSystemId),
    onSuccess: () => {
      message.success('Action created');
      queryClient.invalidateQueries({ queryKey: ['actions'] });
      handleCloseModal();
    },
  });

  const deleteActionMutation = useMutation({
    mutationFn: (id: number) => systemConfigService.deleteAction(id),
    onSuccess: () => {
      message.success('Action deleted');
      queryClient.invalidateQueries({ queryKey: ['actions'] });
    },
  });

  const createDepartmentMutation = useMutation({
    mutationFn: (name: string) => systemConfigService.createDepartment(name),
    onSuccess: () => {
      message.success('Department created');
      queryClient.invalidateQueries({ queryKey: ['departments'] });
      handleCloseModal();
    },
  });

  const deleteDepartmentMutation = useMutation({
    mutationFn: (id: number) => systemConfigService.deleteDepartment(id),
    onSuccess: () => {
      message.success('Department deleted');
      queryClient.invalidateQueries({ queryKey: ['departments'] });
    },
  });

  const handleAdd = () => {
    form.resetFields();
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleFinish = (values: any) => {
    if (activeTab === 'targetSystems') {
      createSystemMutation.mutate({ 
        name: values.name, 
        category: values.category, 
        departmentId: values.departmentId
      });
    } else if (activeTab === 'actions') {
      createActionMutation.mutate({ name: values.name, targetSystemId: values.targetSystemId });
    } else {
      createDepartmentMutation.mutate(values.name);
    }
  };

  const columns = (type: 'system' | 'action' | 'department') => {
    const baseColumns = [
      {
        title: 'Name',
        dataIndex: 'name',
        key: 'name',
      },
      {
        title: 'Value',
        dataIndex: 'value',
        key: 'value',
      },
    ];

    if (type === 'system') {
      baseColumns.splice(2, 0, {
        title: 'Category',
        dataIndex: 'category',
        key: 'category',
        render: (category: string) => category ? <Tag color="blue">{category}</Tag> : '-'
      } as any);
      baseColumns.splice(3, 0, {
        title: 'Department',
        dataIndex: 'departmentId',
        key: 'departmentId',
        render: (id: number) => {
          const dept = departments.find(d => d.id === id);
          return dept ? <Tag color="purple">{dept.name}</Tag> : '-';
        }
      } as any);
    } else if (type === 'action') {
      baseColumns.splice(2, 0, {
        title: 'Target System',
        dataIndex: 'targetSystemId',
        key: 'targetSystemId',
        render: (id: number) => {
          const system = targetSystems.find(s => s.id === id);
          return system ? <Tag color="green">{system.name}</Tag> : '-';
        }
      } as any);
    }

    baseColumns.push({
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: SystemConfigOption) => (
        <Popconfirm
          title="Are you sure to delete this item?"
          onConfirm={() => {
            if (type === 'system') deleteSystemMutation.mutate(record.id);
            else if (type === 'action') deleteActionMutation.mutate(record.id);
            else deleteDepartmentMutation.mutate(record.id);
          }}
          okText="Yes"
          cancelText="No"
        >
          <Button danger icon={<DeleteOutlined />} size="small" />
        </Popconfirm>
      ),
    } as any);

    return baseColumns;
  };

  const items = [
    {
      key: 'targetSystems',
      label: 'Target Systems / Modules',
      children: (
        <Table
          dataSource={targetSystems}
          columns={columns('system')}
          rowKey="id"
          loading={isSystemsLoading}
          pagination={false}
        />
      ),
    },
    {
      key: 'actions',
      label: 'Actions',
      children: (
        <Table
          dataSource={actions}
          columns={columns('action')}
          rowKey="id"
          loading={isActionsLoading}
          pagination={false}
        />
      ),
    },
    {
      key: 'departments',
      label: 'Departments',
      children: (
        <Table
          dataSource={departments}
          columns={columns('department')}
          rowKey="id"
          loading={isDepartmentsLoading}
          pagination={false}
        />
      ),
    },
  ];

  const getModalTitle = () => {
    switch (activeTab) {
      case 'targetSystems': return 'Add New Target System';
      case 'actions': return 'Add New Action';
      case 'departments': return 'Add New Department';
      default: return 'Add New Item';
    }
  };

  return (
    <Card 
      title="System Configuration" 
      extra={
        <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
          {getModalTitle()}
        </Button>
      }
    >
      <Tabs activeKey={activeTab} onChange={setActiveTab} items={items} />

      <Modal
        title={getModalTitle()}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
      >
        <Form form={form} onFinish={handleFinish} layout="vertical">
          <Form.Item
            name="name"
            label="Name"
            rules={[{ required: true, message: 'Please enter a name' }]}
          >
            <Input placeholder="e.g. ERP, Read, etc." />
          </Form.Item>

          {activeTab === 'targetSystems' && (
            <>
              <Form.Item
                name="category"
                label="Category"
                rules={[{ required: true, message: 'Please select a category' }]}
              >
                <Select placeholder="Select category">
                  {SYSTEM_CATEGORIES.map(cat => (
                    <Select.Option key={cat} value={cat}>{cat}</Select.Option>
                  ))}
                </Select>
              </Form.Item>

              <Form.Item
                name="departmentId"
                label="Managing Department"
                rules={[{ required: true, message: 'Please select a department' }]}
              >
                <Select placeholder="Select department">
                  {departments.map(dept => (
                    <Select.Option key={dept.id} value={dept.id}>{dept.name}</Select.Option>
                  ))}
                </Select>
              </Form.Item>
            </>
          )}

          {activeTab === 'actions' && (
            <Form.Item
              name="targetSystemId"
              label="Target System"
              rules={[{ required: true, message: 'Please select a target system' }]}
            >
              <Select placeholder="Select target system">
                {targetSystems.map(sys => (
                  <Select.Option key={sys.id} value={sys.id}>{sys.name}</Select.Option>
                ))}
              </Select>
            </Form.Item>
          )}

          <Form.Item>
            <Space style={{ float: 'right' }}>
              <Button onClick={handleCloseModal}>Cancel</Button>
              <Button type="primary" htmlType="submit" loading={createSystemMutation.isPending || createActionMutation.isPending}>
                Create
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};

export default SystemConfigPage;

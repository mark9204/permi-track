import React, { useState } from 'react';
import { Card, Table, Button, Input, Space, Popconfirm, message, Tabs, Form, Modal } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import systemConfigService from '../services/systemConfigService';
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

  // Mutations
  const createSystemMutation = useMutation({
    mutationFn: (name: string) => systemConfigService.createTargetSystem(name),
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
    mutationFn: (name: string) => systemConfigService.createAction(name),
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

  const handleAdd = () => {
    form.resetFields();
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const handleFinish = (values: { name: string }) => {
    if (activeTab === 'targetSystems') {
      createSystemMutation.mutate(values.name);
    } else {
      createActionMutation.mutate(values.name);
    }
  };

  const columns = (type: 'system' | 'action') => [
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
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: SystemConfigOption) => (
        <Popconfirm
          title="Are you sure to delete this item?"
          onConfirm={() => type === 'system' ? deleteSystemMutation.mutate(record.id) : deleteActionMutation.mutate(record.id)}
          okText="Yes"
          cancelText="No"
        >
          <Button danger icon={<DeleteOutlined />} size="small" />
        </Popconfirm>
      ),
    },
  ];

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
  ];

  return (
    <Card 
      title="System Configuration" 
      extra={
        <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
          Add New {activeTab === 'targetSystems' ? 'System' : 'Action'}
        </Button>
      }
    >
      <Tabs activeKey={activeTab} onChange={setActiveTab} items={items} />

      <Modal
        title={`Add New ${activeTab === 'targetSystems' ? 'Target System' : 'Action'}`}
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

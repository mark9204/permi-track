import React, { useState, useEffect } from 'react';
import { Card, Button, Table, Modal, Form, Select, Input, Tag, message, Space, Popconfirm, InputNumber } from 'antd';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useLocation } from 'react-router-dom';
import accessRequestService from '../services/accessRequestService';
import roleService from '../../roles/services/roleService';
import systemConfigService from '../../admin/services/systemConfigService';
import type { AccessRequest, SubmitRequestPayload } from '../types';
import type { Role } from '../../roles/services/roleService';

const { TextArea } = Input;

const statusTag = (status: string) => {
  switch (status) {
    case 'Pending':
      return <Tag color="processing">Pending</Tag>;
    case 'Approved':
      return <Tag color="success">Approved</Tag>;
    case 'Rejected':
      return <Tag color="error">Rejected</Tag>;
    case 'Cancelled':
    default:
      return <Tag>Cancelled</Tag>;
  }
};

const MyRequestsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const location = useLocation();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    if (location.state?.openCreateModal) {
      setIsModalOpen(true);
      // Optional: Clear state to prevent reopening on refresh? 
      // React Router state persists, so we might want to clear it, 
      // but modifying history state directly is tricky without navigation.
      // For now, this is sufficient.
      window.history.replaceState({}, document.title);
    }
  }, [location.state]);

  const { data: requests = [], isLoading: isRequestsLoading } = useQuery<AccessRequest[]>({ queryKey: ['my-requests'], queryFn: accessRequestService.getMyRequests });

  const { data: roles = [], isLoading: isRolesLoading } = useQuery<Role[]>({ queryKey: ['roles'], queryFn: roleService.getAllRoles, enabled: isModalOpen });

  const { data: targetSystems = [] } = useQuery({
    queryKey: ['targetSystems'],
    queryFn: systemConfigService.getTargetSystems,
    enabled: isModalOpen
  });

  const { data: actions = [] } = useQuery({
    queryKey: ['actions'],
    queryFn: systemConfigService.getActions,
    enabled: isModalOpen
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number) => accessRequestService.cancelRequest(id),
    onSuccess: () => {
      message.success('Request cancelled');
      queryClient.invalidateQueries({ queryKey: ['my-requests'] });
    },
    onError: () => {
      message.error('Failed to cancel request');
    },
  });

  const submitMutation = useMutation({
    mutationFn: (payload: SubmitRequestPayload) => accessRequestService.submitRequest(payload),
    onSuccess: () => {
      message.success('Request submitted');
      setIsModalOpen(false);
      form.resetFields();
      queryClient.invalidateQueries({ queryKey: ['my-requests'] });
    },
    onError: (err: any) => {
      console.error('submitMutation error', err);
      message.error(err?.message || 'Failed to submit request');
    },
  });

  const onCancelRequest = (id: number) => {
    cancelMutation.mutate(id);
  };

  const onOpenModal = () => setIsModalOpen(true);
  const onCloseModal = () => {
    setIsModalOpen(false);
    form.resetFields();
  };

  const onFinish = (values: any) => {
    const payload: SubmitRequestPayload = {
      roleId: Number(values.roleId),
      reason: values.reason,
      requestType: values.requestType,
      targetSystem: values.targetSystem,
      action: values.action,
      durationHours: values.durationHours,
    };
    submitMutation.mutate(payload);
  };

  const columns = [
    {
      title: 'Role',
      dataIndex: 'requestedRoleName',
      key: 'role',
    },
    {
      title: 'Date',
      dataIndex: 'requestedAt',
      key: 'date',
      render: (val: string) => new Date(val).toLocaleDateString(),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (val: string) => statusTag(val),
    },
    {
      title: 'Reason',
      dataIndex: 'reason',
      key: 'reason',
      ellipsis: true,
    },
    {
      title: 'Action',
      key: 'action',
      render: (_: any, record: AccessRequest) => (
        <Space>
          {record.status === 'Pending' && (
            <Popconfirm
              title="Cancel this request?"
              onConfirm={() => onCancelRequest(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Button danger size="small">Cancel</Button>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card
        title="My Access Requests"
        extra={<Button type="primary" onClick={onOpenModal}>New Request</Button>}
      >
        <Table
          rowKey="id"
          columns={columns}
          dataSource={requests}
          loading={isRequestsLoading}
          pagination={{ pageSize: 10 }}
        />
      </Card>

      <Modal title="New Access Request" open={isModalOpen} onCancel={onCloseModal} footer={null}>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item
            label="Type"
            name="requestType"
            rules={[{ required: true, message: 'Please select a type' }]}
          >
            <Select placeholder="Select type">
              <Select.Option value="Systems & Applications">Systems & Applications</Select.Option>
              <Select.Option value="Data & Resources">Data & Resources</Select.Option>
              <Select.Option value="Physical Spaces">Physical Spaces</Select.Option>
              <Select.Option value="Hardware & Assets">Hardware & Assets</Select.Option>
              <Select.Option value="Governance & Authority">Governance & Authority</Select.Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Target System / Module"
            name="targetSystem"
            rules={[{ required: true, message: 'Please select a target system' }]}
          >
            <Select placeholder="Select target system">
              {targetSystems.map((sys: any) => (
                <Select.Option key={sys.id} value={sys.value}>{sys.name}</Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            label="Action"
            name="action"
            rules={[{ required: true, message: 'Please select an action' }]}
          >
            <Select placeholder="Select action">
              {actions.map((act: any) => (
                <Select.Option key={act.id} value={act.value}>{act.name}</Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            label="Duration (Hours) - Optional"
            name="durationHours"
          >
            <InputNumber min={1} style={{ width: '100%' }} placeholder="e.g. 24" />
          </Form.Item>

          <Form.Item
            label="Role"
            name="roleId"
            rules={[{ required: true, message: 'Please select a role' }]}
          >
            <Select
              placeholder="Select role"
              options={roles.map((r: any) => ({ label: r.name, value: r.id }))}
              loading={isRolesLoading}
            />
          </Form.Item>

          <Form.Item label="Reason" name="reason" rules={[{ required: true, message: 'Please enter a reason' }]}> 
            <TextArea rows={4} />
          </Form.Item>

          <Form.Item>
            <Space>
              <Button onClick={onCloseModal}>Cancel</Button>
              <Button type="primary" htmlType="submit" loading={submitMutation.isPending}>Submit</Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default MyRequestsPage;

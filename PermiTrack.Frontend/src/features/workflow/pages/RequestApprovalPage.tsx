import React, { useState } from 'react';
import { Table, Button, Tag, Card, Space, Popconfirm, Modal, Input, message, Tooltip, Typography } from 'antd';
import { CheckOutlined, CloseOutlined } from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import accessRequestService from '../services/accessRequestService';
import type { AccessRequest } from '../types';

const { TextArea } = Input;
const { Text } = Typography;

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

const RequestApprovalPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [rejectModalOpen, setRejectModalOpen] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<AccessRequest | null>(null);
  const [rejectComment, setRejectComment] = useState('');

  const { data: pending = [], isLoading } = useQuery<AccessRequest[]>({ queryKey: ['pending-requests'], queryFn: accessRequestService.getPendingRequests });

  const approveMutation = useMutation({
    mutationFn: (id: number) => accessRequestService.approveRequest(id),
    onSuccess: () => {
      message.success('Request Approved');
      queryClient.invalidateQueries({ queryKey: ['pending-requests'] });
    },
    onError: () => {
      message.error('Failed to approve request');
    },
  });

  const rejectMutation = useMutation({
    mutationFn: ({ id, comment }: { id: number; comment: string }) => accessRequestService.rejectRequest(id, comment),
    onSuccess: () => {
      message.success('Request Rejected');
      setRejectModalOpen(false);
      setRejectComment('');
      setSelectedRequest(null);
      queryClient.invalidateQueries({ queryKey: ['pending-requests'] });
    },
    onError: () => {
      message.error('Failed to reject request');
    },
  });

  const openRejectModal = (record: AccessRequest) => {
    setSelectedRequest(record);
    setRejectComment('');
    setRejectModalOpen(true);
  };

  const handleRejectOk = () => {
    if (!selectedRequest) return;
    if (!rejectComment.trim()) {
      message.error('Please provide a rejection reason');
      return;
    }
    rejectMutation.mutate({ id: selectedRequest.id, comment: rejectComment });
  };

  const columns = [
    {
      title: 'Requester',
      dataIndex: 'requesterName',
      key: 'requester',
      render: (_: any, record: AccessRequest) => (
        <Text>{(record as any).requesterName ?? record.userId}</Text>
      ),
    },
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
      title: 'Reason',
      dataIndex: 'reason',
      key: 'reason',
      ellipsis: true,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (val: string) => statusTag(val),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: any, record: AccessRequest) => (
        <Space>
          {record.status === 'Pending' && (
            <>
              <Popconfirm title="Approve this request?" onConfirm={() => approveMutation.mutate(record.id)} okText="Yes" cancelText="No">
                <Tooltip title="Approve">
                  <Button type="primary" icon={<CheckOutlined />} />
                </Tooltip>
              </Popconfirm>

              <Tooltip title="Reject">
                <Button danger icon={<CloseOutlined />} onClick={() => openRejectModal(record)} />
              </Tooltip>
            </>
          )}
        </Space>
      ),
    },
  ];

  return (
    <Card title="Pending Access Requests" extra={<div />}> 
      <Table rowKey="id" columns={columns} dataSource={pending} loading={isLoading} pagination={{ pageSize: 10 }} />

      <Modal
        title="Reject Request"
        open={rejectModalOpen}
        onOk={handleRejectOk}
        onCancel={() => { setRejectModalOpen(false); setSelectedRequest(null); setRejectComment(''); }}
        okText="Reject"
        cancelText="Cancel"
        confirmLoading={rejectMutation.isPending}
      >
        <TextArea rows={4} value={rejectComment} onChange={(e) => setRejectComment(e.target.value)} placeholder="Enter rejection reason" />
      </Modal>
    </Card>
  );
};

export default RequestApprovalPage;

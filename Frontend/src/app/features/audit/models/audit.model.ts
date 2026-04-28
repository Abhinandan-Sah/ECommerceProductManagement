export interface AuditLog {
  id: string;
  entityName: string;
  entityId: string;
  action: string;
  byUserId: string;
  oldValues: string | null;
  newValues: string | null;
  createdAt: string;
}

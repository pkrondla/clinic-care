import { useQuery } from '@tanstack/react-query';
import reportsService, { 
  type GetCollectionReportParams,
  type GetPatientReportParams,
  type GetInventoryReportParams
} from '@core/services/reportsService';

export function useCollectionReport(params: GetCollectionReportParams) {
  return useQuery({
    queryKey: ['collection-report', params],
    queryFn: () => reportsService.getCollectionReport(params),
    enabled: !!params.startDate && !!params.endDate,
  });
}

export function usePatientReport(params: GetPatientReportParams) {
  return useQuery({
    queryKey: ['patient-report', params],
    queryFn: () => reportsService.getPatientReport(params),
    enabled: !!params.patientId || !!params.startDate,
  });
}

export function useInventoryReport(params: GetInventoryReportParams = {}) {
  return useQuery({
    queryKey: ['inventory-report', params],
    queryFn: () => reportsService.getInventoryReport(params),
  });
}


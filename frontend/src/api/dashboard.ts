import { DashboardResumo } from "../types";
import client from "./client";

export const obterResumoDashboard = (): Promise<DashboardResumo> =>
  client.get<DashboardResumo>("/api/dashboard/resumo").then((r) => r.data);


{{/*
Helm Template Helper Functions
This file contains reusable template functions for the TheDrive Helm chart.
These functions generate consistent names, labels, and selectors across all Kubernetes resources.

Template functions follow Helm best practices and conventions:
- Names are truncated to 63 characters (Kubernetes DNS naming limit)
- Labels follow app.kubernetes.io conventions for better tooling integration
- Functions are prefixed with chart name to avoid conflicts
*/}}

{{/*
Generate the base name of the chart.
This is used as a component in resource names.
Can be overridden by setting nameOverride in values.yaml.
*/}}
{{- define "thedrive.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a fully qualified application name for resource naming.
This ensures unique resource names when multiple releases are installed.

Naming strategy:
1. If fullnameOverride is set in values.yaml, use that
2. If release name contains chart name, use release name only  
3. Otherwise, combine release name and chart name

The 63-character limit ensures compatibility with Kubernetes DNS naming requirements.
*/}}
{{- define "thedrive.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version for the chart label.
Used in metadata labels to identify which chart version created a resource.
Replaces '+' with '_' for Kubernetes label compatibility.
*/}}
{{- define "thedrive.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Generate common labels for all resources.
These labels follow Kubernetes recommended labels:
- helm.sh/chart: Identifies the chart that created the resource
- app.kubernetes.io/name: Name of the application
- app.kubernetes.io/instance: Unique instance identifier (release name)
- app.kubernetes.io/version: Version of the application
- app.kubernetes.io/managed-by: Tool managing the application (Helm)

These labels enable:
- Resource filtering and querying
- Monitoring and observability
- Troubleshooting and debugging
*/}}
{{- define "thedrive.labels" -}}
helm.sh/chart: {{ include "thedrive.chart" . }}
{{ include "thedrive.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Generate selector labels for resource selection.
These labels are used by Services to select Pods and by Deployments to manage Pods.
Must be stable across chart upgrades to maintain service continuity.

Only includes labels that should remain constant:
- app.kubernetes.io/name: Application name
- app.kubernetes.io/instance: Release instance name
*/}}
{{- define "thedrive.selectorLabels" -}}
app.kubernetes.io/name: {{ include "thedrive.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

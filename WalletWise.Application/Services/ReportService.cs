using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WalletWise.Application.Dtos.Reports;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, ICurrentUserService currentUserService, IMapper mapper, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ReportSummaryDto>> GetSummaryAsync()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<ReportSummaryDto>.Failure(DomainErrors.General.Unauthorized);

            var summary = await _reportRepository.GetSummaryAsync(userId);

            return Result<ReportSummaryDto>.Success(_mapper.Map<ReportSummaryDto>(summary));
        }

        public async Task<Result<IEnumerable<MonthlySummaryDto>>> GetMonthlySummaryAsync(int year)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<MonthlySummaryDto>>.Failure(DomainErrors.General.Unauthorized);

            var data = await _reportRepository.GetMonthlySummaryAsync(userId, year);

            return Result<IEnumerable<MonthlySummaryDto>>.Success(_mapper.Map<IEnumerable<MonthlySummaryDto>>(data));
        }

        public async Task<Result<IEnumerable<CategoryReportItemDto>>> GetByCategoryAsync(DateTime start, DateTime end, TypeTransaction? type)
        {
            if (start > end) return Result<IEnumerable<CategoryReportItemDto>>.Failure(DomainErrors.Report.InvalidDateRange.WithMessage("La fecha inicial no puede ser posterior a la fecha final"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId)) return Result<IEnumerable<CategoryReportItemDto>>.Failure(DomainErrors.General.Unauthorized);

            var data = await _reportRepository.GetByCategoryAsync(userId, start, end, type);

            return Result<IEnumerable<CategoryReportItemDto>>.Success(_mapper.Map<IEnumerable<CategoryReportItemDto>>(data));
        }

        public async Task<Result<IEnumerable<TopCategoryReportItemDto>>> GetTopCategoriesAsync(DateTime start, DateTime end, int top)
        {
            if (start > end) return Result<IEnumerable<TopCategoryReportItemDto>>.Failure(DomainErrors.Report.InvalidDateRange.WithMessage("La fecha inicial no puede ser posterior a la fecha final"));
            if (top <= 0) return Result<IEnumerable<TopCategoryReportItemDto>>.Failure(DomainErrors.Report.InvalidLimit.WithMessage("El límite debe ser mayor a 0"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId)) return Result<IEnumerable<TopCategoryReportItemDto>>.Failure(DomainErrors.General.Unauthorized);

            var data = await _reportRepository.GetTopCategoriesAsync(userId, start, end, top);

            return Result<IEnumerable<TopCategoryReportItemDto>>.Success(_mapper.Map<IEnumerable<TopCategoryReportItemDto>>(data));
        }


        public async Task<Result<ComparisonReportDto>> GetComparisonAsync(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            if (startA > endA || startB > endB) return Result<ComparisonReportDto>.Failure(DomainErrors.Report.InvalidDateRange.WithMessage("Las fechas iniciales no pueden ser posteriores a las finales"));

            // Validacion para evitar que se superpongan
            if (startA <= endB && startB <= endA) return Result<ComparisonReportDto>.Failure(DomainErrors.Report.OverlappingPeriods.WithMessage("Los periodos a comparar no deben superponerse"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId)) return Result<ComparisonReportDto>.Failure(DomainErrors.General.Unauthorized);

            var data = await _reportRepository.GetComparisonAsync(userId, startA, endA, startB, endB);

            return Result<ComparisonReportDto>.Success(_mapper.Map<ComparisonReportDto>(data));
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> ExportAsync(DateTime? start, DateTime? end, TypeTransaction? type, int? categoryId, string? search)
        {
            if (start.HasValue && end.HasValue && start > end) return Result<IEnumerable<TransactionResponseDto>>.Failure(DomainErrors.Report.InvalidDateRange.WithMessage("Rango de fechas invalido"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId)) return Result<IEnumerable<TransactionResponseDto>>.Failure(DomainErrors.General.Unauthorized);

            var data = await _reportRepository.GetExportAsync(userId, start, end, type, categoryId, search);

            var dto = _mapper.Map<IEnumerable<TransactionResponseDto>>(data);

            return Result<IEnumerable<TransactionResponseDto>>.Success(dto);
        }
    }
}

# Kiểm tra bản bàn giao

Source giữ nguyên các chức năng của v6.8: ASP.NET Web Forms, ba lớp GUI/BLL/DAL và DTO hỗ trợ. Gói bàn giao chỉ có một file SQL là Database/Setup.sql, tạo database EzCare.

Các kiểm tra đã thực hiện trước khi đóng gói:

- Build bốn project và precompile toàn bộ ASPX/Master đạt.
- 339 kiểm tra nền về nghiệp vụ, chống trùng giờ, mật khẩu/toàn vẹn và HTTP đạt.
- 101 kiểm tra bổ sung về tìm kiếm, phân trang tài khoản đạt với 1.003 tài khoản thử.
- 167 kiểm tra bổ sung về 13 chuyên khoa/26 bác sĩ, đăng nhập, khung giờ và đặt khám từng chuyên khoa đạt.
- 132 kiểm tra bổ sung về quyền admin, đặt hộ, xem/đổi/hủy, tiếp nhận, CSV, hồ sơ và phân quyền các vai trò đạt.

Số kiểm tra HTTP có tính cả tải trang và chuyển hướng, không phải số chức năng riêng. Dữ liệu kiểm thử nằm trong các database tách biệt đã được xóa; không đưa vào gói này.

Bản bàn giao không đổi code nghiệp vụ hoặc dữ liệu EzCare đang chạy. Đã bỏ các script phụ và rà lại hướng dẫn để chỉ dùng Setup.sql khi khởi tạo trên máy mới.

// Sidebar toggle for mobile
document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        document.addEventListener('click', function (e) {
            if (window.innerWidth <= 991 &&
                sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) &&
                !toggleBtn.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        });
    }

    // Auto-dismiss alerts after 5 seconds
    document.querySelectorAll('.custom-alert').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });

    // Photo upload preview
    const photoInput = document.getElementById('PhotoFile');
    const photoPreview = document.getElementById('photoPreview');
    if (photoInput && photoPreview) {
        photoInput.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    photoPreview.src = e.target.result;
                    photoPreview.style.display = 'block';
                };
                reader.readAsDataURL(this.files[0]);
            }
        });
    }

    // Cascading Department -> Designation dropdown
    const deptSelect = document.getElementById('DepartmentId');
    const designationSelect = document.getElementById('DesignationId');
    if (deptSelect && designationSelect) {
        deptSelect.addEventListener('change', function () {
            const departmentId = this.value;
            const currentDesignationId = designationSelect.getAttribute('data-selected') || '';

            if (!departmentId) {
                designationSelect.innerHTML = '<option value="">-- Select Department First --</option>';
                return;
            }

            fetch(`/Employee/GetDesignationsByDepartment?departmentId=${departmentId}`)
                .then(response => response.json())
                .then(data => {
                    designationSelect.innerHTML = '<option value="">-- Select Designation --</option>';
                    data.forEach(item => {
                        const option = document.createElement('option');
                        option.value = item.designationId;
                        option.textContent = item.title;
                        if (item.designationId.toString() === currentDesignationId.toString()) {
                            option.selected = true;
                        }
                        designationSelect.appendChild(option);
                    });
                })
                .catch(err => console.error('Error loading designations:', err));
        });
    }

    // Bootstrap form validation styling
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
});

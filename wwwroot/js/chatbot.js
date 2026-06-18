/**
 * MediCare AI Chatbot — chatbot.js
 * SignalR | Voice Input | PDF Export | Session Management
 */
(function () {
    'use strict';

    // ── State ────────────────────────────────────────────────────
    let connection = null;
    let currentSessionId = null;
    let isOpen = false;
    let isTyping = false;
    let unreadCount = 0;
    let recognition = null;
    let isRecording = false;
    let searchQuery = '';
    let currentRating = 0;
    const TOKEN = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    // ── DOM Refs ─────────────────────────────────────────────────
    const floatBtn      = document.getElementById('chatFloatBtn');
    const popup         = document.getElementById('chatPopup');
    const messagesEl    = document.getElementById('chatMessages');
    const inputEl       = document.getElementById('chatInput');
    const sendBtn       = document.getElementById('chatSendBtn');
    const voiceBtn      = document.getElementById('chatVoiceBtn');
    const typingEl      = document.getElementById('chatTyping');
    const sessionsBar   = document.getElementById('chatSessionsBar');
    const searchBar     = document.getElementById('chatSearchBar');
    const searchInput   = document.getElementById('chatSearchInput');
    const charCounter   = document.getElementById('charCounter');
    const unreadBadge   = document.getElementById('chatUnreadBadge');
    const welcomeScreen = document.getElementById('chatWelcome');

    // ── Init ─────────────────────────────────────────────────────
    async function init() {
        setupSignalR();
        setupEventListeners();
        setupVoiceInput();
        await loadSessions();
    }

    // ── SignalR Setup ────────────────────────────────────────────
    function setupSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub')
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        connection.on('MessageSaved', (data) => {
            // Update session title chip
            if (data.sessionTitle && data.sessionTitle !== 'New Conversation') {
                updateSessionChip(currentSessionId, data.sessionTitle);
            }
        });

        connection.on('AITyping', (show) => {
            typingEl.style.display = show ? 'flex' : 'none';
            if (show) scrollToBottom();
            isTyping = show;
            sendBtn.disabled = show;
        });

        connection.on('ReceiveAIMessage', (data) => {
            appendMessage('AI', data.message, data.createdAt, data.isEmergency);
            showRating();
            if (!isOpen) incrementUnread();
            scrollToBottom();
        });

        // ── Agent rich card handler ─────────────────────────────────
        connection.on('ReceiveAgentAction', (data) => {
            switch (data.actionType) {
                case 'doctor_list':          appendDoctorList(data.actionData);     break;
                case 'slot_picker':          appendSlotPicker(data.actionData);     break;
                case 'appointment_confirmed':appendApptConfirm(data.actionData);    break;
                case 'appointment_list':     appendApptList(data.actionData);       break;
                case 'patient_profile':      appendPatientProfile(data.actionData); break;
            }
            scrollToBottom();
        });

        connection.on('Error', (msg) => {
            showToast(msg, 'error');
            sendBtn.disabled = false;
        });

        connection.onreconnecting(() => showToast('Reconnecting...', 'info'));
        connection.onreconnected(() => showToast('Connected!', 'success'));

        connection.start().catch(err => console.error('SignalR error:', err));
    }

    // ── Event Listeners ──────────────────────────────────────────
    function setupEventListeners() {
        // Toggle popup
        floatBtn.addEventListener('click', toggleChat);

        // Send on button click
        sendBtn.addEventListener('click', sendMessage);

        // Send on Enter (Shift+Enter = newline)
        inputEl.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        // Auto-resize textarea & char counter
        inputEl.addEventListener('input', () => {
            inputEl.style.height = 'auto';
            inputEl.style.height = Math.min(inputEl.scrollHeight, 80) + 'px';
            const len = inputEl.value.length;
            charCounter.textContent = `${len}/2000`;
            charCounter.classList.toggle('warn', len > 1800);
        });

        // Header action buttons
        document.getElementById('chatSearchToggle')?.addEventListener('click', toggleSearch);
        document.getElementById('chatNewBtn')?.addEventListener('click', newSession);
        document.getElementById('chatExportBtn')?.addEventListener('click', exportPdf);
        document.getElementById('chatClearBtn')?.addEventListener('click', clearChat);
        document.getElementById('chatCloseBtn')?.addEventListener('click', toggleChat);

        // Search
        searchInput?.addEventListener('input', (e) => {
            searchQuery = e.target.value.toLowerCase();
            filterMessages(searchQuery);
        });

        // New chat chip
        document.getElementById('newChatChip')?.addEventListener('click', newSession);

        // Quick reply chips
        document.querySelectorAll('.cpw-chip').forEach(chip => {
            chip.addEventListener('click', () => {
                inputEl.value = chip.dataset.text;
                sendMessage();
            });
        });

        // Star rating
        document.querySelectorAll('.star-btn').forEach(btn => {
            btn.addEventListener('click', async () => {
                currentRating = parseInt(btn.dataset.star);
                document.querySelectorAll('.star-btn').forEach((b, i) => {
                    b.classList.toggle('active', i < currentRating);
                });
                await submitRating(currentRating);
                showToast('Thank you for your feedback! ⭐', 'success');
            });
        });
    }

    // ── Toggle Chat ──────────────────────────────────────────────
    function toggleChat() {
        isOpen = !isOpen;
        popup.classList.toggle('open', isOpen);
        floatBtn.classList.toggle('open', isOpen);
        if (isOpen) {
            resetUnread();
            inputEl.focus();
            if (!currentSessionId) newSession();
        }
    }

    // ── Session Management ───────────────────────────────────────
    async function loadSessions() {
        try {
            const res = await fetch('/Chat/Sessions');
            const sessions = await res.json();
            renderSessionChips(sessions);
            if (sessions.length > 0) {
                await switchSession(sessions[0].id);
            }
        } catch (e) { console.error('Failed to load sessions', e); }
    }

    function renderSessionChips(sessions) {
        sessionsBar.querySelectorAll('.cp-chip:not(.cp-chip-new)').forEach(c => c.remove());
        const newChip = document.getElementById('newChatChip');
        sessions.forEach(s => {
            const chip = document.createElement('div');
            chip.className = 'cp-chip';
            chip.dataset.sessionId = s.id;
            chip.innerHTML = `<span class="chip-title">${escapeHtml(s.title)}</span><span class="chip-del" data-id="${s.id}" title="Delete">✕</span>`;
            chip.addEventListener('click', (e) => {
                if (e.target.classList.contains('chip-del')) { deleteSession(s.id); }
                else { switchSession(s.id); }
            });
            sessionsBar.insertBefore(chip, newChip);
        });
    }

    async function switchSession(sessionId) {
        currentSessionId = sessionId;
        document.querySelectorAll('.cp-chip').forEach(c =>
            c.classList.toggle('active', parseInt(c.dataset.sessionId) === sessionId));
        messagesEl.innerHTML = '';
        hideWelcome();

        try {
            const res = await fetch(`/Chat/History/${sessionId}`);
            const data = await res.json();
            data.messages.forEach(m => appendMessage(m.sender, m.message, m.createdAt, m.isEmergency, false));
            if (data.messages.length === 0) showWelcome();
            scrollToBottom();
        } catch (e) { console.error('Failed to load history', e); }
    }

    async function newSession() {
        try {
            const res = await fetch('/Chat/NewSession', {
                method: 'POST',
                headers: { 'RequestVerificationToken': TOKEN, 'Content-Type': 'application/json' }
            });
            const data = await res.json();
            if (data.success) {
                currentSessionId = data.sessionId;
                messagesEl.innerHTML = '';
                showWelcome();
                const newChip = document.getElementById('newChatChip');
                const chip = document.createElement('div');
                chip.className = 'cp-chip active';
                chip.dataset.sessionId = data.sessionId;
                chip.innerHTML = `<span class="chip-title">New Conversation</span><span class="chip-del" data-id="${data.sessionId}" title="Delete">✕</span>`;
                chip.addEventListener('click', (e) => {
                    if (e.target.classList.contains('chip-del')) deleteSession(data.sessionId);
                    else switchSession(data.sessionId);
                });
                sessionsBar.insertBefore(chip, newChip);
                document.querySelectorAll('.cp-chip').forEach(c =>
                    c.classList.toggle('active', parseInt(c.dataset.sessionId) === data.sessionId));
                hideRating();
                inputEl.focus();
            }
        } catch (e) { showToast('Failed to create session', 'error'); }
    }

    function updateSessionChip(sessionId, title) {
        const chip = sessionsBar.querySelector(`[data-session-id="${sessionId}"] .chip-title`);
        if (chip) chip.textContent = title;
    }

    async function deleteSession(sessionId) {
        if (!confirm('Delete this conversation?')) return;
        try {
            await fetch(`/Chat/DeleteSession/${sessionId}`, {
                method: 'POST',
                headers: { 'RequestVerificationToken': TOKEN }
            });
            sessionsBar.querySelector(`[data-session-id="${sessionId}"]`)?.remove();
            if (currentSessionId === sessionId) {
                const remaining = sessionsBar.querySelector('.cp-chip:not(.cp-chip-new)');
                if (remaining) switchSession(parseInt(remaining.dataset.sessionId));
                else { currentSessionId = null; messagesEl.innerHTML = ''; showWelcome(); }
            }
        } catch (e) { showToast('Failed to delete session', 'error'); }
    }

    // ── Send Message ─────────────────────────────────────────────
    async function sendMessage() {
        const msg = inputEl.value.trim();
        if (!msg || isTyping) return;
        if (!currentSessionId) { showToast('Starting new session...'); await newSession(); }
        if (connection.state !== signalR.HubConnectionState.Connected) {
            showToast('Reconnecting to server...', 'error');
            try { await connection.start(); } catch { return; }
        }

        hideWelcome();
        appendMessage('User', msg, formatTime(new Date()), false);
        inputEl.value = '';
        inputEl.style.height = 'auto';
        charCounter.textContent = '0/2000';
        sendBtn.disabled = true;
        scrollToBottom();

        try {
            await connection.invoke('SendMessage', currentSessionId, msg);
        } catch (err) {
            showToast('Failed to send message', 'error');
            sendBtn.disabled = false;
        }
    }

    // ── Render Message Bubble ────────────────────────────────────
    function appendMessage(sender, text, time, isEmergency, animate = true) {
        const isUser = sender === 'User';
        const wrapper = document.createElement('div');
        wrapper.className = `chat-msg ${isUser ? 'user' : 'ai'}${animate ? '' : ' no-anim'}`;

        const copyBtn = !isUser ? `<button class="copy-btn" title="Copy">⎘</button>` : '';

        wrapper.innerHTML = `
            <div class="msg-avatar">${isUser ? '👤' : 'A'}</div>
            <div class="msg-body">
                <div class="msg-bubble ${isEmergency ? 'emergency' : ''}">
                    ${copyBtn}
                    ${renderMarkdown(text)}
                </div>
                <div class="msg-time">${time}</div>
            </div>`;

        // Copy button logic
        wrapper.querySelector('.copy-btn')?.addEventListener('click', () => {
            navigator.clipboard.writeText(text).then(() => showToast('Copied!', 'success'));
        });

        messagesEl.appendChild(wrapper);
        if (!animate) wrapper.style.animation = 'none';
    }

    // ── Minimal markdown renderer ────────────────────────────────
    function renderMarkdown(text) {
        return escapeHtml(text)
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>')
            .replace(/^(#{1,3})\s+(.+)$/gm, (_, h, t) => `<strong>${t}</strong>`)
            .replace(/^[-•]\s+(.+)$/gm, '<li>$1</li>')
            .replace(/(<li>.*<\/li>)+/gs, m => `<ul>${m}</ul>`)
            .replace(/\n{2,}/g, '</p><p>')
            .replace(/\n/g, '<br>');
    }

    // ── Search ───────────────────────────────────────────────────
    function toggleSearch() {
        searchBar?.classList.toggle('show');
        if (searchBar?.classList.contains('show')) searchInput?.focus();
        else { searchQuery = ''; filterMessages(''); }
    }

    function filterMessages(query) {
        document.querySelectorAll('.chat-msg').forEach(msg => {
            const text = msg.querySelector('.msg-bubble')?.textContent.toLowerCase() || '';
            msg.style.display = !query || text.includes(query) ? 'flex' : 'none';
        });
    }

    // ── Voice Input ──────────────────────────────────────────────
    function setupVoiceInput() {
        const SR = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SR) { voiceBtn?.setAttribute('title', 'Voice input not supported'); voiceBtn && (voiceBtn.style.opacity = '0.4'); return; }

        recognition = new SR();
        recognition.continuous = false;
        recognition.interimResults = true;
        recognition.lang = 'en-US';

        recognition.onresult = (e) => {
            const transcript = Array.from(e.results).map(r => r[0].transcript).join('');
            inputEl.value = transcript;
        };

        recognition.onend = () => { isRecording = false; voiceBtn?.classList.remove('recording'); };
        recognition.onerror = (e) => {
            isRecording = false;
            voiceBtn?.classList.remove('recording');
            const msgs = {
                'audio-capture'  : 'No microphone found. Please connect a mic and try again.',
                'not-allowed'    : 'Microphone access denied. Please allow mic permission in your browser.',
                'network'        : 'Network error during voice recognition. Check your connection.',
                'no-speech'      : 'No speech detected. Please try speaking again.',
                'aborted'        : null,   // user cancelled — no toast needed
            };
            const msg = msgs[e.error];
            if (msg) showToast(msg, 'warning');
            // Hide voice button entirely if mic hardware is unavailable
            if (e.error === 'audio-capture') {
                if (voiceBtn) { voiceBtn.style.display = 'none'; }
            }
        };

        voiceBtn?.addEventListener('click', () => {
            if (isRecording) { recognition.stop(); }
            else { recognition.start(); isRecording = true; voiceBtn.classList.add('recording'); }
        });
    }

    // ── PDF Export ───────────────────────────────────────────────
    function exportPdf() {
        if (!currentSessionId) { showToast('No active session', 'error'); return; }
        window.open(`/Chat/ExportPdf/${currentSessionId}`, '_blank');
    }

    // ── Clear Chat ───────────────────────────────────────────────
    async function clearChat() {
        if (!currentSessionId || !confirm('Start a new conversation?')) return;
        await newSession();
    }

    // ── Rating ───────────────────────────────────────────────────
    function showRating() { document.getElementById('chatRating')?.style.setProperty('display', 'flex'); }
    function hideRating() {
        document.getElementById('chatRating')?.style.setProperty('display', 'none');
        currentRating = 0;
        document.querySelectorAll('.star-btn').forEach(b => b.classList.remove('active'));
    }

    async function submitRating(stars) {
        if (!currentSessionId) return;
        try {
            await fetch('/Chat/Rate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': TOKEN },
                body: JSON.stringify({ sessionId: currentSessionId, stars })
            });
        } catch (e) { console.error('Rating failed', e); }
    }

    // ── Helpers ──────────────────────────────────────────────────
    function scrollToBottom() {
        setTimeout(() => { messagesEl.scrollTop = messagesEl.scrollHeight; }, 50);
    }

    function showWelcome()  { welcomeScreen && (welcomeScreen.style.display = 'flex'); }
    function hideWelcome()  { welcomeScreen && (welcomeScreen.style.display = 'none'); }

    function incrementUnread() {
        unreadCount++;
        unreadBadge.textContent = unreadCount > 9 ? '9+' : unreadCount;
        unreadBadge.classList.add('show');
    }

    function resetUnread() {
        unreadCount = 0;
        unreadBadge.classList.remove('show');
    }

    function escapeHtml(str) {
        return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

    function formatTime(date) {
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    }

    function showToast(msg, type = 'info') {
        let toast = document.getElementById('chatToast');
        if (!toast) { toast = document.createElement('div'); toast.id = 'chatToast'; toast.className = 'chat-toast'; document.body.appendChild(toast); }
        toast.textContent = msg;
        toast.className = `chat-toast show ${type}`;
        clearTimeout(toast._timer);
        toast._timer = setTimeout(() => toast.classList.remove('show'), 3000);
    }

    // ══ AGENT RICH CARD RENDERERS ══════════════════════════════════════

    function agentCard(html) {
        const wrap = document.createElement('div');
        wrap.className = 'agent-card-wrap';
        wrap.innerHTML = html;
        messagesEl.appendChild(wrap);
        return wrap;
    }

    // ── Doctor List Card ─────────────────────────────────────────────
    function appendDoctorList(data) {
        if (!data.doctors || data.doctors.length === 0) return;
        const cards = data.doctors.map(d => `
            <div class="agent-doctor-card">
                <div class="adc-avatar">${escapeHtml(d.name.charAt(0))}</div>
                <div class="adc-info">
                    <div class="adc-name">${escapeHtml(d.name)}</div>
                    <div class="adc-spec">${escapeHtml(d.specialization)}</div>
                    <div class="adc-meta">
                        <span>🏥 ${escapeHtml(d.department)}</span>
                        ${d.experienceYears ? `<span>⏳ ${d.experienceYears}yr exp</span>` : ''}
                        <span>💰 ৳${d.fee}</span>
                    </div>
                    ${d.chamberAddress ? `<div class="adc-address">📍 ${escapeHtml(d.chamberAddress)}</div>` : ''}
                </div>
                <button class="adc-book-btn" onclick="agentBookDoctor(${d.id}, '${escapeHtml(d.name)}', '${data.date||''}')">
                    📅 Book
                </button>
            </div>`).join('');
        agentCard(`
            <div class="agent-section-label">👨‍⚕️ Doctors Found (${data.doctors.length})</div>
            <div class="agent-doctor-grid">${cards}</div>`);
    }

    // ── Slot Picker Card ─────────────────────────────────────────────
    function appendSlotPicker(data) {
        if (!data.slots || data.slots.length === 0) return;
        const slots = data.slots.map(s => `
            <button class="agent-slot-btn ${s.isAvailable ? '' : 'unavailable'}"
                    ${s.isAvailable ? `onclick="agentSelectSlot(${data.doctorId}, '${escapeHtml(data.doctorName)}', '${data.date}', '${s.time}', '${s.displayTime}')"` : 'disabled'}
                    title="${s.isAvailable ? 'Click to book' : 'Already booked'}">
                ${s.displayTime}
            </button>`).join('');
        agentCard(`
            <div class="agent-section-label">⏰ Available Slots — ${escapeHtml(data.doctorName)} | ${escapeHtml(data.date)}</div>
            <div class="agent-slot-wrap">${slots}</div>`);
    }

    // ── Appointment Confirm Card ───────────────────────────────────────
    function appendApptConfirm(appt) {
        const payBtn = appt.paymentUrl
            ? `<a href="${appt.paymentUrl}" class="acc-pay-btn" target="_self">💳 Pay Now — ৳${appt.fee}</a>`
            : `<div class="acc-pay-note">✅ Appointment reserved. Complete payment from <a href="/User/MyAppointments">My Appointments</a>.</div>`;

        agentCard(`
            <div class="agent-confirm-card">
                <div class="acc-icon">🗓️</div>
                <div class="acc-body">
                    <div class="acc-title">Appointment Booked!</div>
                    <div class="acc-ref">${escapeHtml(appt.appointmentNo)}</div>
                    <div class="acc-row"><span>👨‍⚕️ Doctor</span><strong>${escapeHtml(appt.doctorName)}</strong></div>
                    <div class="acc-row"><span>🩺 Specialty</span><strong>${escapeHtml(appt.specialization)}</strong></div>
                    <div class="acc-row"><span>📅 Date</span><strong>${escapeHtml(appt.date)}</strong></div>
                    <div class="acc-row"><span>⏰ Time</span><strong>${escapeHtml(appt.time)}</strong></div>
                    <div class="acc-row"><span>💰 Fee</span><strong>৳${appt.fee}</strong></div>
                    ${appt.chiefComplaint ? `<div class="acc-row"><span>📋 Reason</span><strong>${escapeHtml(appt.chiefComplaint)}</strong></div>` : ''}
                    <div class="acc-pay-wrap">${payBtn}</div>
                </div>
            </div>`);
    }

    // ── Appointment List Card ──────────────────────────────────────────
    function appendApptList(data) {
        if (!data.appointments || data.appointments.length === 0) return;
        const rows = data.appointments.map(a => `
            <div class="agent-appt-row">
                <div class="aar-left">
                    <div class="aar-doc">${escapeHtml(a.doctorName)}</div>
                    <div class="aar-spec">${escapeHtml(a.specialization)}</div>
                    <div class="aar-dt">📅 ${escapeHtml(a.date)} &nbsp;⏰ ${escapeHtml(a.time)}</div>
                </div>
                <div class="aar-right">
                    <span class="aar-status ${a.status.toLowerCase()}">${a.status}</span>
                    <button class="aar-cancel-btn" onclick="agentCancelAppt(${a.id}, '${escapeHtml(a.appointmentNo)}')">❌ Cancel</button>
                </div>
            </div>`).join('');
        agentCard(`
            <div class="agent-section-label">📊 Your Upcoming Appointments (${data.appointments.length})</div>
            <div class="agent-appt-list">${rows}</div>`);
    }

    // ── Patient Profile Card ───────────────────────────────────────────
    function appendPatientProfile(p) {
        agentCard(`
            <div class="agent-profile-card">
                <div class="apc-avatar">${escapeHtml((p.fullName||'?').charAt(0))}</div>
                <div class="apc-body">
                    <div class="apc-name">${escapeHtml(p.fullName)}</div>
                    <div class="apc-no">${escapeHtml(p.patientNo)}</div>
                    <div class="apc-grid">
                        ${p.gender    ? `<div><span>Gender</span><strong>${escapeHtml(p.gender)}</strong></div>` : ''}
                        ${p.dateOfBirth ? `<div><span>DOB</span><strong>${escapeHtml(p.dateOfBirth)}</strong></div>` : ''}
                        ${p.bloodGroup ? `<div><span>Blood</span><strong>${escapeHtml(p.bloodGroup)}</strong></div>` : ''}
                        ${p.mobileNumber ? `<div><span>Phone</span><strong>${escapeHtml(p.mobileNumber)}</strong></div>` : ''}
                        ${p.email ? `<div><span>Email</span><strong>${escapeHtml(p.email)}</strong></div>` : ''}
                        ${p.address ? `<div><span>Address</span><strong>${escapeHtml(p.address)}</strong></div>` : ''}
                    </div>
                </div>
            </div>`);
    }

    // ══ AGENT ACTION HANDLERS (button callbacks) ═══════════════════════════

    // Called when user clicks "Book" on a doctor card
    window.agentBookDoctor = function(doctorId, doctorName, preferredDate) {
        const date = preferredDate || new Date().toISOString().split('T')[0];
        const msg  = `Show me available slots for Dr. ${doctorName} on ${date}`;
        inputEl.value = msg;
        sendMessage();
    };

    // Called when user clicks a time slot button — directly books + initiates payment
    window.agentSelectSlot = async function(doctorId, doctorName, date, time, displayTime) {
        // Ask for symptoms first
        const symptoms = prompt(
            `📋 Briefly describe your symptoms or reason for visiting Dr. ${doctorName}:\n(Press Cancel to abort booking)`);
        if (symptoms === null) return; // user cancelled

        showToast('Booking your appointment and preparing payment...', 'info');

        try {
            const res = await fetch('/api/agent/book-with-payment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({
                    doctorId,
                    date,
                    time,
                    chiefComplaint: symptoms || null
                })
            });

            const data = await res.json();
            if (data.success) {
                hideWelcome();
                appendApptConfirm(data.appointment);
                scrollToBottom();
                showToast('Appointment booked! Complete your payment below.', 'success');
            } else {
                showToast(data.error || 'Booking failed. Please try again.', 'error');
            }
        } catch {
            showToast('Network error. Please try again.', 'error');
        }
    };

    // Called when user clicks Cancel on an appointment row
    window.agentCancelAppt = async function(appointmentId, apptNo) {
        if (!confirm(`Cancel appointment ${apptNo}?`)) return;
        try {
            const res = await fetch(`/api/agent/appointments/${appointmentId}`, {
                method: 'DELETE',
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });
            const data = await res.json();
            if (data.success) {
                showToast('Appointment cancelled.', 'success');
                // Send a follow-up message to update context
                setTimeout(() => {
                    inputEl.value = 'Show my upcoming appointments';
                    sendMessage();
                }, 800);
            } else {
                showToast(data.error || 'Failed to cancel.', 'error');
            }
        } catch { showToast('Network error. Please try again.', 'error'); }
    };

    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('meta[name="csrf-token"]')?.content
            || '';
    }

    // ── Boot ─────────────────────────────────────────────────────
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

})();

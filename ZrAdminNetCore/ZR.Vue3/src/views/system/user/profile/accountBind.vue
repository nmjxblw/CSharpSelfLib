<template>
  <div class="app-container gradient-card">
    <div class="item">
      <div class="icon-wrapper">
        <svg-icon name="gitee" size="24px"></svg-icon>
      </div>
      <div class="info">
        <div class="info-top">
          <span class="label">绑定Gitee</span>
          <span class="bind">
            <svg-icon name="ele-check" color="#1ab394"></svg-icon>
            <span class="text-navy">已绑定</span>
          </span>
        </div>
        <div class="info-desc">可通过Gitee进行登录</div>
      </div>
      <div class="btn-wrapper">
        <el-button @click="handleBind('gitee')">绑定</el-button>
      </div>
    </div>
    <div class="item">
      <div class="icon-wrapper">
        <svg-icon name="github"></svg-icon>
      </div>
      <div class="info">
        <div class="info-top">
          <span class="label">绑定Github</span>
          <span class="bind"></span>
        </div>
        <div class="info-desc">可通过Github进行登录</div>
      </div>
      <div class="btn-wrapper">
        <el-button @click="handleBind('github')">绑定</el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
const { proxy } = getCurrentInstance()

function handleBind(type) {
  proxy.$modal.msgError('暂未实现')
}
</script>
<style lang="scss" scoped>
.gradient-card .item {
  align-items: center;
  display: flex;
  margin-bottom: 20px;
}

.gradient-card .item .icon-wrapper {
  align-items: center;
  background: #ccc;
  border-radius: 50%;
  display: flex;
  height: 48px;
  justify-content: center;
  width: 48px;
}

.gradient-card .item .info {
  flex: 1 1;
  margin: 0 16px;
}

.gradient-card .item .info-top {
  margin-bottom: 4px;
}

.gradient-card .item .info-top .label {
  font-weight: 500;
  line-height: 22px;
  margin-right: 12px;
}

.gradient-card .item .info-top .bind {
  font-size: 12px;
  font-weight: 500;
  line-height: 20px;
}

.gradient-card .item .info-desc {
  color: #86909c;
  font-size: 12px;
  font-weight: 400;
  line-height: 20px;
}

.gradient-card .item .info-desc .value {
  color: #4e5969;
}

.gradient-card .item .btn-wrapper {
  align-self: flex-start;
}

.gradient-card .item .btn-wrapper .btn {
  height: 28px;
  width: 56px;
}

.gradient-card .item .btn-wrapper .btn:hover {
  -webkit-box-shadow: 0 2px 3px rgba(0, 0, 0, 0.15);
  box-shadow: 0 2px 3px #00000026;
  border-color: rgb(var(--primary-5));
}

.gradient-card .detail {
  display: flex;
  font-size: 12px;
  justify-content: flex-start;
  margin: -5px 0 0 64px;
}

.gradient-card .detail .sub-text-wrapper {
  width: 100%;
  border-left: 2px solid var(--color-fill-4);
  padding-left: 12px;
}
</style>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Magma.Infiniband.Interop
{
    internal static class IbvDevice
    {
        [DllImport("libibverbs")]
        public unsafe static extern ibv_device** ibv_get_device_list(out int num_devices);

        [DllImport("libibverbs")]
        public unsafe static extern void ibv_free_device_list(ref ibv_device* device_list);

        private const int IBV_SYSFS_NAME_MAX = 64;
        private const int IBV_SYSFS_PATH_MAX = 256;

        public enum ibv_node_type
        {
            IBV_NODE_UNKNOWN = -1,
            IBV_NODE_CA = 1,
            IBV_NODE_SWITCH,
            IBV_NODE_ROUTER,
            IBV_NODE_RNIC,
            IBV_NODE_USNIC,
            IBV_NODE_USNIC_UDP,
        }

        public enum ibv_transport_type
        {
            IBV_TRANSPORT_UNKNOWN = -1,
            IBV_TRANSPORT_IB = 0,
            IBV_TRANSPORT_IWARP,
            IBV_TRANSPORT_USNIC,
            IBV_TRANSPORT_USNIC_UDP,
        }

        public enum ibv_device_cap_flags
        {
            IBV_DEVICE_RESIZE_MAX_WR = 1,
            IBV_DEVICE_BAD_PKEY_CNTR = 1 << 1,
            IBV_DEVICE_BAD_QKEY_CNTR = 1 << 2,
            IBV_DEVICE_RAW_MULTI = 1 << 3,
            IBV_DEVICE_AUTO_PATH_MIG = 1 << 4,
            IBV_DEVICE_CHANGE_PHY_PORT = 1 << 5,
            IBV_DEVICE_UD_AV_PORT_ENFORCE = 1 << 6,
            IBV_DEVICE_CURR_QP_STATE_MOD = 1 << 7,
            IBV_DEVICE_SHUTDOWN_PORT = 1 << 8,
            IBV_DEVICE_INIT_TYPE = 1 << 9,
            IBV_DEVICE_PORT_ACTIVE_EVENT = 1 << 10,
            IBV_DEVICE_SYS_IMAGE_GUID = 1 << 11,
            IBV_DEVICE_RC_RNR_NAK_GEN = 1 << 12,
            IBV_DEVICE_SRQ_RESIZE = 1 << 13,
            IBV_DEVICE_N_NOTIFY_CQ = 1 << 14,
            IBV_DEVICE_MEM_WINDOW = 1 << 17,
            IBV_DEVICE_UD_IP_CSUM = 1 << 18,
            IBV_DEVICE_XRC = 1 << 20,
            IBV_DEVICE_MEM_MGT_EXTENSIONS = 1 << 21,
            IBV_DEVICE_MEM_WINDOW_TYPE_2A = 1 << 23,
            IBV_DEVICE_MEM_WINDOW_TYPE_2B = 1 << 24,
            IBV_DEVICE_RC_IP_CSUM = 1 << 25,
            IBV_DEVICE_RAW_IP_CSUM = 1 << 26,
            IBV_DEVICE_MANAGED_FLOW_STEERING = 1 << 29
        }

        public enum ibv_port_state
        {
            IBV_PORT_NOP = 0,
            IBV_PORT_DOWN = 1,
            IBV_PORT_INIT = 2,
            IBV_PORT_ARMED = 3,
            IBV_PORT_ACTIVE = 4,
            IBV_PORT_ACTIVE_DEFER = 5
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct ibv_device
        {
            private IntPtr _legacy;
            private IntPtr _legacy2;
            public ibv_node_type NodeType;
            public ibv_transport_type TransportType;
            private fixed byte _name[IBV_SYSFS_NAME_MAX];
            private fixed byte _deviceName[IBV_SYSFS_NAME_MAX];
            private fixed byte _devicePath[IBV_SYSFS_PATH_MAX];
            private fixed byte _ibDevicePath[IBV_SYSFS_PATH_MAX];

            public override string ToString()
            {
                fixed (byte* namePtr = _name)
                fixed (byte* devNamePtr = _deviceName)
                {
                    var name = Encoding.UTF8.GetString(namePtr, IBV_SYSFS_NAME_MAX);
                    var devName = Encoding.UTF8.GetString(devNamePtr, IBV_SYSFS_NAME_MAX);
                    return $"{name} - DevName {devName}";
                }
            }
        }
    }
}

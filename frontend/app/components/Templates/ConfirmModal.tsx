import React, { useEffect, useState } from 'react';
import { Template } from '@/app/types/Template';
import { FormData } from '@/app/pages/AddPage';
import { 
    StyleSheet, 
    Text, 
    View, 
    DeviceEventEmitter,
    Modal, 
    TextInput, 
    KeyboardAvoidingView, 
    Platform, 
    ScrollView, 
    Keyboard 
} from 'react-native';
import { useTestUser } from '@/app/context/TestUserContext';
import CreatableSelect from '../CreatableSelect';
import { BouncyPressable } from '../BouncyPressable';
import { useGlobalButtons } from './ButtonProvider';
import { useApi } from '@/app/context/ApiContext';
import { ExpensePayload } from './TemplateButton';

interface ConfirmModalProps {
    isVisible: boolean
    setModalVisible: (visibility: boolean) => void
    template: Template | null
    groups: string[]
    categories: string[]
    tags: string[]
}

export default function ConfirmModal({ 
    isVisible, 
    setModalVisible, 
    template,
    groups = [],
    categories = [],
    tags = []
}: ConfirmModalProps) {
    const { isForTestUser, activeSpreadsheetId } = useTestUser()
    const { triggerAction } = useGlobalButtons();
    const api = useApi()

    const [form, setForm] = useState<FormData>({
        name: template!.name,
        group: template!.group,
        category: template!.category,
        tag: template!.tag,
        amount: String(template!.amount),
        description: template!.description
    })

    useEffect(() => {
        if (template && isVisible) {
            setForm({
                name: template.name || '',
                group: template.group || '',
                category: template.category || '',
                tag: template.tag || '',
                amount: template.amount > -1 ? String(template.amount) : '',
                description: template.description || ''
            });
        }
    }, [template, isVisible]);

    const handleChange = (field: string, value: string) => {
        setForm((prev) => ({...prev, [field]: value}))
    }

    const handleOnSavePressed = async () => {
        Keyboard.dismiss()

        triggerAction(async () => {
            try {
                const payload: ExpensePayload = {
                    name: form.name,
                    group: form.group,
                    category: form.category,
                    tag: form.tag,
                    amount: Number(form.amount),
                    description: form.description.trim().length === 0 ? '' : form.description
                }

                setModalVisible(false)

                await api.post(
                    `/api/v1/expenses?spreadsheetid=${activeSpreadsheetId}&sheet=expenses`,
                    payload
                )

                DeviceEventEmitter.emit('expenseAdded');
            } catch (error: any) {
                console.error("Failed to add expense: ", error);

                if (error.response) {
                    console.log("Error data:", error.response.data);
                    console.log("Error status:", error.response.status);
                }
            }
        })
    }

    return (
        <Modal
            animationType='none'
            transparent={true}
            visible={isVisible}
            onRequestClose={() => setModalVisible(false)}
        >
            <View style={styles.backdrop}>
                <KeyboardAvoidingView
                    style={styles.backdrop}
                    behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
                >
                    <ScrollView
                        contentContainerStyle={styles.scrollContent}
                        keyboardShouldPersistTaps="handled"
                        showsVerticalScrollIndicator={false}
                    >
                        <View style={styles.modalCard}>
                            <View style={[styles.form, styles.formGap]}>
                                <View>
                                    <Text style={styles.inputLabel}>Name</Text>
                                    <TextInput 
                                        style={styles.input}
                                        value={form.name}
                                        onChangeText={(val) => handleChange('name', val)}
                                    />
                                </View>
            
                                <View style={[styles.reverseContainer, styles.formGap]}>
                                    <View style={styles.buttonContainer}>
                                        <BouncyPressable
                                            onPress={() => {
                                                Keyboard.dismiss()
                                                setModalVisible(false)
                                            }}
                                            style={[styles.saveButton, {backgroundColor: isForTestUser ? 'red' : 'black'}]}
                                        >
                                            <Text style={styles.saveButtonText}>Cancel</Text>
                                        </BouncyPressable>

                                        <BouncyPressable
                                            onPress={handleOnSavePressed}
                                            style={[styles.saveButton, {backgroundColor: 'black'}]}
                                        >
                                            <Text style={styles.saveButtonText}>Save</Text>
                                        </BouncyPressable>
                                    </View>
            
                                    <View>
                                        <Text style={styles.inputLabel}>Description</Text>
                                        <TextInput 
                                            style={styles.input}
                                            value={form.description}
                                            onChangeText={(val) => handleChange('description', val)}
                                        />
                                    </View>
            
                                    <View>
                                        <Text style={styles.inputLabel}>Amount</Text>
                                        <TextInput 
                                            style={styles.input}
                                            value={form.amount}
                                            onChangeText={(val) => handleChange('amount', val)}
                                        />
                                    </View>
            
                                    { !isForTestUser &&
                                        <CreatableSelect 
                                            label="Tag"
                                            value={form.tag}
                                            onChangeText={(val: string) => handleChange('tag', val)}
                                            options={tags}
                                        />
                                    }

                                    { !isForTestUser &&
                                        <CreatableSelect 
                                            label="Category"
                                            value={form.category}
                                            onChangeText={(val: string) => handleChange('category', val)}
                                            options={categories}
                                        />
                                    }

                                    { !isForTestUser &&
                                        <CreatableSelect 
                                            label="Group"
                                            value={form.group}
                                            onChangeText={(val: string) => handleChange('group', val)}
                                            options={groups}
                                        />
                                    }
                                </View>
                            </View>
                        </View>
                    </ScrollView>
                </KeyboardAvoidingView>
            </View>
        </Modal>
    )
}

const styles = StyleSheet.create({
    backdrop: {
        flex: 1,
        backgroundColor: 'rgba(0, 0, 0, 0.5)', // Dim overlay
    },
    modalCard: {
        width: '100%',
        backgroundColor: '#ffffff',
        borderRadius: 12,
        alignItems: 'center',
        padding: 20,
        gap: 16,
        elevation: 5, // Shadow for Android
        shadowColor: '#000', // Shadow for iOS
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.25,
        shadowRadius: 4,
    },
    input: {
        height: 44,
        borderWidth: 0.2,
        borderColor: '#8E8E8E',
        borderRadius: 8,
        paddingHorizontal: 12,
        fontSize: 15,
        color: '#000000',
        backgroundColor: '#FFFFFF',
    },
    inputLabel: {
        fontWeight: 600,
        marginBottom: 6,
    },
    form: {
        marginTop: 24,
        marginBottom: 20,
        width: '100%'
    },
    formGap: {
        gap: 20
    },
    reverseContainer: {
        flexDirection: 'column-reverse'
    },
    buttonContainer: {
        marginTop: 20,
        gap: 10
    },
    saveButton: {
        alignItems: 'center',
        fontSize: 18,
        paddingVertical: 12,
        borderRadius: 8,
    },
    saveButtonText: {
        color: 'white',
    },
    scrollContent: {
        flexGrow: 1,
        justifyContent: 'center',
        alignItems: 'center',
        padding: 20,
    },
})